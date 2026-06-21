using System.Data;
using inzynierka.Data;
using inzynierka.Products.Model;
using inzynierka.Products.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Npgsql;
using NpgsqlTypes;

namespace inzynierka.Products.OpenFoodFacts.Repositories
{
    public class OpenFoodFactsRepository : IProductBulkRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<OpenFoodFactsRepository> _logger;

        public OpenFoodFactsRepository(AppDbContext context, ILogger<OpenFoodFactsRepository> logger)
        {
            _context = context;
            _logger  = logger;
        }

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

        public async Task<int> BulkImportBatchAsync(ProductBatch batch, CancellationToken ct = default)
        {
            if (batch.IsEmpty) return 0;

            // Deduplicate products within the batch (by Code, case-insensitive).
            // On duplicate codes keep the newest record (by LastUpdated), consistent
            // with the conflict-resolution rule in the INSERT ... ON CONFLICT below.
            var products = batch.Products
                .Where(p => !string.IsNullOrWhiteSpace(p.Code))
                .GroupBy(p => p.Code!.Trim(), StringComparer.OrdinalIgnoreCase)
                .Select(g => g.OrderByDescending(p => p.LastUpdated ?? DateTime.MinValue).First())
                .ToList();

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
                // synchronous_commit=off drops the fsync on commit for throughput.
                // Safe because the import is idempotent and resumable (ON CONFLICT/MERGE):
                // a batch lost to a server crash is recovered by re-running the import.
                await ExecAsync(conn, tx, @"
                    SET LOCAL statement_timeout = 0;
                    SET LOCAL lock_timeout = 0;
                    SET LOCAL idle_in_transaction_session_timeout = 0;
                    SET LOCAL synchronous_commit = off;", ct);

                var imported = await UpsertProductsAsync(conn, tx, products, ct);

                await UpsertLinksAsync(conn, tx, "stage_prod_ingredient", "ProductIngredientTag", "ProductId", "IngredientTagId", "IngredientTags", batch.IngredientLinks, ct);
                await UpsertLinksAsync(conn, tx, "stage_prod_country",    "ProductCountryTag",    "ProductId", "CountryTagId",    "CountryTags",    batch.CountryLinks,    ct);
                await UpsertLinksAsync(conn, tx, "stage_prod_category",   "ProductCategoryTag",   "ProductId", "CategoryTagId",   "CategoryTags",   batch.CategoryLinks,   ct);
                await UpsertLinksAsync(conn, tx, "stage_prod_allergen",   "ProductAllergenTag",   "ProductId", "AllergenTagId",   "AllergenTags",   batch.AllergenLinks,   ct);

                await tx.CommitAsync(ct);
                return imported;
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync(ct);
                _logger.LogError(ex, "BulkImportBatchAsync failed (products={Products})", products.Count);
                throw;
            }
            finally
            {
                if (mustClose) await conn.CloseAsync();
            }
        }

        #region Products (staging + binary COPY + INSERT ON CONFLICT)

        private static async Task<int> UpsertProductsAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx, List<Product> products, CancellationToken ct)
        {
            if (products.Count == 0) return 0;

            await ExecAsync(conn, tx, @"
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
                    last_updated         timestamp        NULL
                ) ON COMMIT DROP;", ct);

            const string copyCmd = @"
                COPY products_stage
                (
                    code, language, brand_owner, language_code, product_name, brands,
                    nutrition_grade, nova_group, eco_score_grade, ingredients_text,
                    serving_size, is_vegetarian, is_vegan, image_url,
                    energy_100g, energy_kcal_100g, fat_100g, saturated_fat_100g,
                    carbohydrates_100g, sugars_100g, fiber_100g, proteins_100g,
                    salt_100g, sodium_100g, energy_kcal_serving, last_updated
                ) FROM STDIN (FORMAT BINARY)";

            await using (var w = await conn.BeginBinaryImportAsync(copyCmd, ct))
            {
                foreach (var p in products)
                {
                    var code = p.Code?.Trim();
                    if (string.IsNullOrEmpty(code)) continue;

                    await w.StartRowAsync(ct);
                    await Text(w, code, ct);
                    await Text(w, p.Language?.Trim(), ct);
                    await Text(w, p.BrandOwner?.Trim(), ct);
                    await Text(w, p.LanguageCode?.Trim(), ct);
                    await Text(w, p.ProductName?.Trim(), ct);
                    await Text(w, p.Brands?.Trim(), ct);
                    await Text(w, p.NutritionGrade?.Trim(), ct);
                    await Int(w, p.NovaGroup, ct);
                    await Text(w, p.EcoScoreGrade?.Trim(), ct);
                    await Text(w, p.IngredientsText?.Trim(), ct);
                    await Text(w, p.ServingSize?.Trim(), ct);
                    await Text(w, p.IsVegetarian?.Trim(), ct);
                    await Text(w, p.IsVegan?.Trim(), ct);
                    await Text(w, p.ImageUrl?.Trim(), ct);
                    await Double(w, p.Energy100g, ct);
                    await Double(w, p.EnergyKcal100g, ct);
                    await Double(w, p.Fat100g, ct);
                    await Double(w, p.SaturatedFat100g, ct);
                    await Double(w, p.Carbohydrates100g, ct);
                    await Double(w, p.Sugars100g, ct);
                    await Double(w, p.Fiber100g, ct);
                    await Double(w, p.Proteins100g, ct);
                    await Double(w, p.Salt100g, ct);
                    await Double(w, p.Sodium100g, ct);
                    await Double(w, p.EnergyKcalServing, ct);
                    await Timestamp(w, p.LastUpdated, ct);
                }

                await w.CompleteAsync(ct);
            }

            // No ANALYZE: the upsert is a seq-scan of the stage + INSERT ON CONFLICT, no JOIN to tune.
            // The returned count = rows actually inserted or updated (rows skipped by the WHERE in
            // DO UPDATE are not counted), i.e. the real number of writes to "Products".
            return await ExecAsync(conn, tx, @"
                INSERT INTO ""Products""
                (
                    ""Code"", ""Language"", ""BrandOwner"", ""LanguageCode"", ""ProductName"",
                    ""Brands"", ""NutritionGrade"", ""NovaGroup"", ""EcoScoreGrade"",
                    ""IngredientsText"", ""ServingSize"", ""IsVegetarian"", ""IsVegan"",
                    ""ImageUrl"", ""Energy100g"", ""EnergyKcal100g"", ""Fat100g"",
                    ""SaturatedFat100g"", ""Carbohydrates100g"", ""Sugars100g"", ""Fiber100g"",
                    ""Proteins100g"", ""Salt100g"", ""Sodium100g"", ""EnergyKcalServing"",
                    ""LastUpdated"", ""Source""
                )
                SELECT
                    s.code, s.language, s.brand_owner, s.language_code, s.product_name,
                    s.brands, s.nutrition_grade, s.nova_group, s.eco_score_grade,
                    s.ingredients_text, s.serving_size, s.is_vegetarian, s.is_vegan,
                    s.image_url, s.energy_100g, s.energy_kcal_100g, s.fat_100g,
                    s.saturated_fat_100g, s.carbohydrates_100g, s.sugars_100g, s.fiber_100g,
                    s.proteins_100g, s.salt_100g, s.sodium_100g, s.energy_kcal_serving,
                    s.last_updated, 0
                FROM products_stage s
                WHERE s.code IS NOT NULL AND btrim(s.code) <> ''
                ON CONFLICT (""Code"") DO UPDATE SET
                    ""ProductName""       = EXCLUDED.""ProductName"",
                    ""BrandOwner""        = EXCLUDED.""BrandOwner"",
                    ""Brands""            = EXCLUDED.""Brands"",
                    ""Language""          = EXCLUDED.""Language"",
                    ""LanguageCode""      = EXCLUDED.""LanguageCode"",
                    ""IngredientsText""   = EXCLUDED.""IngredientsText"",
                    ""NutritionGrade""    = EXCLUDED.""NutritionGrade"",
                    ""NovaGroup""         = EXCLUDED.""NovaGroup"",
                    ""EcoScoreGrade""     = EXCLUDED.""EcoScoreGrade"",
                    ""ServingSize""       = EXCLUDED.""ServingSize"",
                    ""IsVegetarian""      = EXCLUDED.""IsVegetarian"",
                    ""IsVegan""           = EXCLUDED.""IsVegan"",
                    ""Energy100g""        = EXCLUDED.""Energy100g"",
                    ""EnergyKcal100g""    = EXCLUDED.""EnergyKcal100g"",
                    ""Fat100g""           = EXCLUDED.""Fat100g"",
                    ""SaturatedFat100g""  = EXCLUDED.""SaturatedFat100g"",
                    ""Carbohydrates100g"" = EXCLUDED.""Carbohydrates100g"",
                    ""Sugars100g""        = EXCLUDED.""Sugars100g"",
                    ""Fiber100g""         = EXCLUDED.""Fiber100g"",
                    ""Proteins100g""      = EXCLUDED.""Proteins100g"",
                    ""Salt100g""          = EXCLUDED.""Salt100g"",
                    ""Sodium100g""        = EXCLUDED.""Sodium100g"",
                    ""EnergyKcalServing"" = EXCLUDED.""EnergyKcalServing"",
                    ""LastUpdated""       = EXCLUDED.""LastUpdated""
                WHERE EXCLUDED.""LastUpdated"" > ""Products"".""LastUpdated""
                   OR ""Products"".""LastUpdated"" IS NULL;", ct);
        }

        #endregion

        #region Links (staging + binary COPY + tag ensure + MERGE)

        private static async Task UpsertLinksAsync(
            NpgsqlConnection conn, NpgsqlTransaction tx,
            string tempName, string joinTable, string joinProdCol, string joinTagCol, string tagTable,
            List<(string Code, string TagName)> links, CancellationToken ct)
        {
            if (links.Count == 0) return;

            await ExecAsync(conn, tx, $@"
                CREATE TEMP TABLE IF NOT EXISTS {tempName}
                (
                    code     text NOT NULL,
                    tag_name text NOT NULL
                ) ON COMMIT DROP;", ct);

            await using (var w = await conn.BeginBinaryImportAsync($@"COPY {tempName} (code, tag_name) FROM STDIN (FORMAT BINARY)", ct))
            {
                foreach (var (code, tag) in links)
                {
                    var c = code?.Trim();
                    var t = tag?.Trim();
                    if (string.IsNullOrEmpty(c) || string.IsNullOrEmpty(t)) continue;

                    await w.StartRowAsync(ct);
                    await w.WriteAsync(c, NpgsqlDbType.Text, ct);
                    await w.WriteAsync(t, NpgsqlDbType.Text, ct);
                }

                await w.CompleteAsync(ct);
            }

            // ANALYZE potrzebny: poniższy MERGE JOIN-uje stage z Products (miliony wierszy).
            await ExecAsync(conn, tx, $@"ANALYZE {tempName};", ct);

            // Dosianie brakujących tagów wprost ze stage'a. Idempotentne bez unikalnego
            // indeksu na Name (single-writer): NOT EXISTS + DISTINCT ON po znormalizowanej nazwie.
            await ExecAsync(conn, tx, $@"
                INSERT INTO ""{tagTable}"" (""Name"")
                SELECT DISTINCT ON (upper(btrim(s.tag_name))) btrim(s.tag_name)
                FROM {tempName} s
                WHERE btrim(s.tag_name) <> ''
                  AND NOT EXISTS (
                      SELECT 1 FROM ""{tagTable}"" t
                      WHERE upper(btrim(t.""Name"")) = upper(btrim(s.tag_name))
                  )
                ORDER BY upper(btrim(s.tag_name));", ct);

            await ExecAsync(conn, tx, $@"
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
                AND jt.""{joinTagCol}""  = src.""{joinTagCol}""
                WHEN NOT MATCHED THEN
                  INSERT (""{joinProdCol}"", ""{joinTagCol}"")
                  VALUES (src.""{joinProdCol}"", src.""{joinTagCol}"");", ct);
        }

        #endregion

        #region Low-level helpers

        private static async Task<int> ExecAsync(NpgsqlConnection conn, NpgsqlTransaction tx, string sql, CancellationToken ct)
        {
            await using var cmd = new NpgsqlCommand(sql, conn, tx) { CommandTimeout = 0 };
            return await cmd.ExecuteNonQueryAsync(ct);
        }

        private static Task Text(NpgsqlBinaryImporter w, string? v, CancellationToken ct) =>
            v is null ? w.WriteNullAsync(ct) : w.WriteAsync(v, NpgsqlDbType.Text, ct);

        private static Task Int(NpgsqlBinaryImporter w, int? v, CancellationToken ct) =>
            v is null ? w.WriteNullAsync(ct) : w.WriteAsync(v.Value, NpgsqlDbType.Integer, ct);

        private static Task Double(NpgsqlBinaryImporter w, double? v, CancellationToken ct) =>
            v is null ? w.WriteNullAsync(ct) : w.WriteAsync(v.Value, NpgsqlDbType.Double, ct);

        private static Task Timestamp(NpgsqlBinaryImporter w, DateTime? v, CancellationToken ct) =>
            v is null
                ? w.WriteNullAsync(ct)
                // Kolumna to `timestamp without time zone`; znacznik Unspecified pasuje do tego typu
                // (ta sama liczba mikrosekund, bez konwersji strefy).
                : w.WriteAsync(DateTime.SpecifyKind(v.Value, DateTimeKind.Unspecified), NpgsqlDbType.Timestamp, ct);

        #endregion
    }
}
