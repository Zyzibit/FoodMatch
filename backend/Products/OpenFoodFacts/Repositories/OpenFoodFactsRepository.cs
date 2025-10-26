using System.Data;
using EFCore.BulkExtensions;
using inzynierka.Data;
using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace inzynierka.Products.OpenFoodFacts.Repositories
{
    public class OpenFoodFactsRepository : IOpenFoodFactsRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OpenFoodFactsRepository> _logger;

        public OpenFoodFactsRepository(AppDbContext context, ILogger<OpenFoodFactsRepository> logger)
        {
            _context = context;
            _logger  = logger;
        }

        #region Bulk link helpers 

        private static async Task CreateAndFillStageAsync(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            string tempName,
            IEnumerable<(string code, string tag)> rows,
            CancellationToken ct)
        {
            var createTemp = $@"
                CREATE TEMP TABLE IF NOT EXISTS {tempName}
                (
                    code     text NOT NULL,
                    tag_name text NOT NULL
                ) ON COMMIT DROP;";

            await using (var cmd = new NpgsqlCommand(createTemp, conn, tx) { CommandTimeout = 0 })
                await cmd.ExecuteNonQueryAsync(ct);

            var copy = $@"COPY {tempName} (code, tag_name) FROM STDIN (FORMAT BINARY)";
            await using var writer = await conn.BeginBinaryImportAsync(copy, ct);

            foreach (var (code, tag) in rows)
            {
                var c = code?.Trim();
                var t = tag?.Trim();
                if (string.IsNullOrEmpty(c) || string.IsNullOrEmpty(t)) continue;

                writer.WriteRow(new object?[] { c, t });
            }

            await writer.CompleteAsync(ct);
        }

        private static string BuildJoinMergeSql(
            string tempName,
            string joinTable,
            string joinProdCol,
            string joinTagCol,
            string tagTable)
        {
            // DISTINCT limits comparisons with a large number of duplicates in staging
            return $@"
                MERGE INTO ""{joinTable}"" AS jt
                USING (
                    SELECT DISTINCT
                           p.""Id"" AS ""{joinProdCol}"",
                           t.""Id"" AS ""{joinTagCol}""
                    FROM {tempName} s
                    JOIN ""Products"" p
                      ON upper(btrim(p.""Code"")) = upper(btrim(s.code))
                    JOIN ""{tagTable}"" t
                      ON upper(btrim(t.""Name"")) = upper(btrim(s.tag_name))
                ) AS src
                ON  jt.""{joinProdCol}"" = src.""{joinProdCol}""
                AND jt.""{joinTagCol}"" = src.""{joinTagCol}""
                WHEN NOT MATCHED THEN
                  INSERT (""{joinProdCol}"", ""{joinTagCol}"")
                  VALUES (src.""{joinProdCol}"", src.""{joinTagCol}"");";
        }

        private static async Task ExecuteLinkMergeAsync(
            NpgsqlConnection conn,
            NpgsqlTransaction tx,
            string tempName,
            string joinTable,
            string joinProdCol,
            string joinTagCol,
            string tagTable,
            CancellationToken ct)
        {
            await using (var cfg = new NpgsqlCommand(@"
                SET LOCAL statement_timeout = 0;
                SET LOCAL lock_timeout = 0;
                SET LOCAL idle_in_transaction_session_timeout = 0;
                SET LOCAL synchronous_commit = off;", conn, tx) { CommandTimeout = 0 })
            {
                await cfg.ExecuteNonQueryAsync(ct);
            }

            // Planner will better estimate costs
            await using (var analyze = new NpgsqlCommand($@"ANALYZE {tempName};", conn, tx) { CommandTimeout = 0 })
                await analyze.ExecuteNonQueryAsync(ct);

            var sql = BuildJoinMergeSql(tempName, joinTable, joinProdCol, joinTagCol, tagTable);

            await using var cmd = new NpgsqlCommand(sql, conn, tx) { CommandTimeout = 0 };
            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task BulkUpsertProductIngredientLinksAsync(
            List<(string Code, string TagName)> items,
            CancellationToken ct = default)
        {
            if (items.Count == 0) return;

            var conn  = (NpgsqlConnection)_context.Database.GetDbConnection();
            var close = false;

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
                close = true;
            }

            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                const string temp = "stage_prod_ingredient";
                await CreateAndFillStageAsync(conn, tx, temp, items, ct);

                await ExecuteLinkMergeAsync(
                    conn, tx,
                    tempName: temp,
                    joinTable: "ProductIngredientTag",
                    joinProdCol: "ProductId",
                    joinTagCol: "IngredientTagId",
                    tagTable: "IngredientTags",
                    ct: ct);

                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (close) await conn.CloseAsync();
            }
        }

        public async Task BulkUpsertProductCountryLinksAsync(
            List<(string Code, string TagName)> items,
            CancellationToken ct = default)
        {
            if (items.Count == 0) return;

            var conn  = (NpgsqlConnection)_context.Database.GetDbConnection();
            var close = false;

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
                close = true;
            }

            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                const string temp = "stage_prod_country";
                await CreateAndFillStageAsync(conn, tx, temp, items, ct);

                await ExecuteLinkMergeAsync(
                    conn, tx,
                    tempName: temp,
                    joinTable: "ProductCountryTag",
                    joinProdCol: "ProductId",
                    joinTagCol: "CountryTagId",
                    tagTable: "CountryTags",
                    ct: ct);

                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (close) await conn.CloseAsync();
            }
        }

        public async Task BulkUpsertProductCategoryLinksAsync(
            List<(string Code, string TagName)> items,
            CancellationToken ct = default)
        {
            if (items.Count == 0) return;

            var conn  = (NpgsqlConnection)_context.Database.GetDbConnection();
            var close = false;

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
                close = true;
            }

            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                const string temp = "stage_prod_category";
                await CreateAndFillStageAsync(conn, tx, temp, items, ct);

                await ExecuteLinkMergeAsync(
                    conn, tx,
                    tempName: temp,
                    joinTable: "ProductCategoryTag",
                    joinProdCol: "ProductId",
                    joinTagCol: "CategoryTagId",
                    tagTable: "CategoryTags",
                    ct: ct);

                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (close) await conn.CloseAsync();
            }
        }

        public async Task BulkUpsertProductAllergenLinksAsync(
            List<(string Code, string TagName)> items,
            CancellationToken ct = default)
        {
            if (items.Count == 0) return;

            var conn  = (NpgsqlConnection)_context.Database.GetDbConnection();
            var close = false;

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
                close = true;
            }

            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                const string temp = "stage_prod_allergen";
                await CreateAndFillStageAsync(conn, tx, temp, items, ct);

                await ExecuteLinkMergeAsync(
                    conn, tx,
                    tempName: temp,
                    joinTable: "ProductAllergenTag",
                    joinProdCol: "ProductId",
                    joinTagCol: "AllergenTagId",
                    tagTable: "AllergenTags",
                    ct: ct);

                await tx.CommitAsync(ct);
            }
            catch
            {
                await tx.RollbackAsync(ct);
                throw;
            }
            finally
            {
                if (close) await conn.CloseAsync();
            }
        }

        #endregion

        #region Bulk import switches

        public void PrepareForBulkImport()
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = false;
            _context.ChangeTracker.QueryTrackingBehavior    = QueryTrackingBehavior.NoTracking;
            _context.Database.SetCommandTimeout(TimeSpan.FromHours(12));
        }

        public void RestoreAfterBulkImport()
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = true;
            _context.ChangeTracker.QueryTrackingBehavior    = QueryTrackingBehavior.TrackAll;
            _context.Database.SetCommandTimeout(null);
        }

        #endregion

        #region Tags

        public async Task<Dictionary<string, int>> GetTagDictionaryAsync<T>(CancellationToken ct = default)
            where T : class, ITagEntity
        {
            var rows = await _context.Set<T>()
                .AsNoTracking()
                .Select(t => new { t.Id, t.Name })
                .ToListAsync(ct);

            var dict = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            foreach (var r in rows)
            {
                if (string.IsNullOrWhiteSpace(r.Name)) continue;

                var key = r.Name.Trim();

                if (!dict.TryAdd(key, r.Id))
                    _logger.LogWarning("Duplicate tag in {TagType}: {Name}", typeof(T).Name, key);
            }

            return dict;
        }

        private static string NormalizeTagName(string name) => name.Trim();

        public async Task BulkEnsureTagsAsync<T>(
            IReadOnlyCollection<string> tagNames,
            CancellationToken ct = default)
            where T : class, ITagEntity, new()
        {
            if (tagNames.Count == 0) return;

            var clean = tagNames
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Select(NormalizeTagName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (clean.Count == 0) return;

            var existing = await _context.Set<T>()
                .AsNoTracking()
                .Where(t => clean.Contains(t.Name))
                .Select(t => t.Name)
                .ToListAsync(ct);

            var missing = clean
                .Except(existing, StringComparer.OrdinalIgnoreCase)
                .Select(n => new T { Name = n })
                .ToList();

            if (missing.Count == 0) return;

            await _context.Set<T>().AddRangeAsync(missing, ct);
            await _context.SaveChangesAsync(ct);
        }

        #endregion

        #region Products (staging + COPY + INSERT ON CONFLICT DO NOTHING)

        public async Task BulkInsertProductsAsync(List<Product> batch, CancellationToken ct = default)
        {
            var rows = batch
                .Where(p => !string.IsNullOrWhiteSpace(p.Code))
                .GroupBy(p => p.Code!.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .ToList();

            if (rows.Count == 0) return;

            var conn      = (NpgsqlConnection)_context.Database.GetDbConnection();
            var mustClose = false;

            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync(ct);
                mustClose = true;
            }

            await using var tx = await conn.BeginTransactionAsync(ct);

            try
            {
                // Session configuration for long bulk transaction
                await using (var cfg = new NpgsqlCommand(@"
                    SET LOCAL statement_timeout = 0;
                    SET LOCAL lock_timeout = 0;
                    SET LOCAL idle_in_transaction_session_timeout = 0;
                    SET LOCAL synchronous_commit = off;", conn, tx) { CommandTimeout = 0 })
                {
                    await cfg.ExecuteNonQueryAsync(ct);
                }

                const string createTemp = @"
                    CREATE TEMP TABLE IF NOT EXISTS products_stage
                    (
                        code                 text             NOT NULL,
                        language             text             NULL,
                        brand_owner          text             NULL,
                        language_code        text             NULL,
                        product_name         text             NULL,
                        brands               text             NULL,
                        nutrition_grade      text             NULL,
                        nova_group           integer          NULL,
                        eco_score_grade      text             NULL,
                        ingredients_text     text             NULL,
                        serving_size         text             NULL,
                        is_vegetarian        text             NULL,
                        is_vegan             text             NULL,
                        image_url            text             NULL,
                        energy_100g          double precision NULL,
                        energy_kcal_100g     double precision NULL,
                        fat_100g             double precision NULL,
                        saturated_fat_100g   double precision NULL,
                        carbohydrates_100g   double precision NULL,
                        sugars_100g          double precision NULL,
                        fiber_100g           double precision NULL,
                        proteins_100g        double precision NULL,
                        salt_100g            double precision NULL,
                        sodium_100g          double precision NULL,
                        energy_kcal_serving  double precision NULL,
                        last_updated         timestamp        NULL,
                        is_ai_generated      boolean          NOT NULL DEFAULT false
                    ) ON COMMIT DROP;";

                await using (var cmd = new NpgsqlCommand(createTemp, conn, tx) { CommandTimeout = 0 })
                    await cmd.ExecuteNonQueryAsync(ct);

                const string copyCmd = @"
                    COPY products_stage
                    (
                        code,
                        language,
                        brand_owner,
                        language_code,
                        product_name,
                        brands,
                        nutrition_grade,
                        nova_group,
                        eco_score_grade,
                        ingredients_text,
                        serving_size,
                        is_vegetarian,
                        is_vegan,
                        image_url,
                        energy_100g,
                        energy_kcal_100g,
                        fat_100g,
                        saturated_fat_100g,
                        carbohydrates_100g,
                        sugars_100g,
                        fiber_100g,
                        proteins_100g,
                        salt_100g,
                        sodium_100g,
                        energy_kcal_serving,
                        last_updated
                    ) FROM STDIN (FORMAT BINARY)";

                await using (var writer = await conn.BeginBinaryImportAsync(copyCmd, ct))
                {
                    foreach (var p in rows)
                    {
                        var code = p.Code?.Trim();
                        if (string.IsNullOrEmpty(code)) continue;

                        writer.WriteRow(new object?[]
                        {
                            code,
                            p.Language?.Trim(),
                            p.BrandOwner?.Trim(),
                            p.LanguageCode?.Trim(),
                            p.ProductName?.Trim(),
                            p.Brands?.Trim(),
                            p.NutritionGrade?.Trim(),
                            p.NovaGroup,
                            p.EcoScoreGrade?.Trim(),
                            p.IngredientsText?.Trim(),
                            p.ServingSize?.Trim(),
                            p.IsVegetarian?.Trim(),
                            p.IsVegan?.Trim(),
                            p.ImageUrl?.Trim(),
                            p.Energy100g,
                            p.EnergyKcal100g,
                            p.Fat100g,
                            p.SaturatedFat100g,
                            p.Carbohydrates100g,
                            p.Sugars100g,
                            p.Fiber100g,
                            p.Proteins100g,
                            p.Salt100g,
                            p.Sodium100g,
                            p.EnergyKcalServing,
                            p.LastUpdated
                        });
                    }

                    await writer.CompleteAsync(ct);
                }

                // Optionally ANALYZE staging
                await using (var analyze = new NpgsqlCommand(@"ANALYZE products_stage;", conn, tx) { CommandTimeout = 0 })
                    await analyze.ExecuteNonQueryAsync(ct);

                const string upsert = @"
                    INSERT INTO ""Products""
                    (
                        ""Code"",
                        ""Language"",
                        ""BrandOwner"",
                        ""LanguageCode"",
                        ""ProductName"",
                        ""Brands"",
                        ""NutritionGrade"",
                        ""NovaGroup"",
                        ""EcoScoreGrade"",
                        ""IngredientsText"",
                        ""ServingSize"",
                        ""IsVegetarian"",
                        ""IsVegan"",
                        ""ImageUrl"",
                        ""Energy100g"",
                        ""EnergyKcal100g"",
                        ""Fat100g"",
                        ""SaturatedFat100g"",
                        ""Carbohydrates100g"",
                        ""Sugars100g"",
                        ""Fiber100g"",
                        ""Proteins100g"",
                        ""Salt100g"",
                        ""Sodium100g"",
                        ""EnergyKcalServing"",
                        ""LastUpdated"",
                        ""IsAiGenerated""
                    )
                    SELECT
                        s.code,
                        s.language,
                        s.brand_owner,
                        s.language_code,
                        s.product_name,
                        s.brands,
                        s.nutrition_grade,
                        s.nova_group,
                        s.eco_score_grade,
                        s.ingredients_text,
                        s.serving_size,
                        s.is_vegetarian,
                        s.is_vegan,
                        s.image_url,
                        s.energy_100g,
                        s.energy_kcal_100g,
                        s.fat_100g,
                        s.saturated_fat_100g,
                        s.carbohydrates_100g,
                        s.sugars_100g,
                        s.fiber_100g,
                        s.proteins_100g,
                        s.salt_100g,
                        s.sodium_100g,
                        s.energy_kcal_serving,
                        s.last_updated,
                        s.is_ai_generated
                    FROM products_stage s
                    WHERE s.code IS NOT NULL AND btrim(s.code) <> ''
                    ON CONFLICT (""Code"") DO NOTHING;";

                await using (var cmd2 = new NpgsqlCommand(upsert, conn, tx) { CommandTimeout = 0 })
                    await cmd2.ExecuteNonQueryAsync(ct);

                await tx.CommitAsync(ct);
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "BulkInsertProductsAsync failed (rows={Rows})", rows.Count);
                throw;
            }
            finally
            {
                if (mustClose) await conn.CloseAsync();
            }
        }

        public async Task BulkInsertJoinsAsync<TJoin>(List<TJoin> joins, CancellationToken ct = default)
            where TJoin : class
        {
            if (joins.Count == 0) return;

            await _context.BulkInsertAsync(joins, new BulkConfig
            {
                BatchSize           = 50_000,
                UseTempDB           = true,
                PreserveInsertOrder = false,
                SetOutputIdentity   = false,
                IncludeGraph        = false,
                BulkCopyTimeout     = 0
            }, cancellationToken: ct);
        }

        #endregion
    }
}
