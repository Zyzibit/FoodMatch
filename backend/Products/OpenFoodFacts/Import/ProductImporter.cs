using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.CountryTag;
using inzynierka.Products.Model.Tag.IngredientTag;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;
using inzynierka.Products.OpenFoodFacts.Repositories;
using Microsoft.Extensions.Logging;

namespace inzynierka.Products.OpenFoodFacts.Import
{
    /// <summary>
    /// Importer OFF → PostgreSQL: JSONL → batch → mapowanie → repo (COPY/UPSERT).
    /// Produkty: COPY + INSERT ON CONFLICT("CodeNorm") DO NOTHING
    /// Tagi: BulkEnsureTagsAsync
    /// Relacje: staging COPY + INSERT SELECT + ON CONFLICT DO NOTHING
    /// </summary>
    public sealed class ProductImporter : IProductImporter
    {
        private readonly IOpenFoodFactsRepository _repo;
        private readonly ILogger<ProductImporter> _logger;

        private const int ProductBatchSize = 50_000;
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            AllowTrailingCommas = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip
        };

        public ProductImporter(IOpenFoodFactsRepository repo, ILogger<ProductImporter> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task ImportJsonlAsync(string filePath, CancellationToken ct = default)
        {
            _repo.PrepareForBulkImport();

            try
            {
                var products = new List<Product>(ProductBatchSize);
                var tags = new TagBuffers(ProductBatchSize);

                var processed = 0;
                var readLines = 0;

                await foreach (var rawLine in ReadLinesAsync(filePath, ct))
                {
                    readLines++;
                    if (string.IsNullOrWhiteSpace(rawLine)) continue;

                    var src = TryDeserialize(rawLine);
                    if (src is null) continue;

                    var code = Sanitizer(src.Code);
                    if (string.IsNullOrWhiteSpace(code)) continue;
                    code = code!.Trim();

                    products.Add(MapToProduct(src));
                    CollectTags(src, code, tags);

                    if (products.Count < ProductBatchSize) continue;

                    await FlushAsync(products, tags, ct);
                    processed += products.Count;
                    _logger.LogInformation("Imported so far: {Count} products (read lines: {Read})", processed, readLines);

                    products.Clear();
                    tags.Clear();
                }

                if (products.Count > 0)
                {
                    await FlushAsync(products, tags, ct);
                    processed += products.Count;
                }

                _logger.LogInformation("Import finished. Total imported products (attempted): {Count}", processed);
            }
            finally
            {
                _repo.RestoreAfterBulkImport();
            }
        }

        // ========================= Parsowanie / mapowanie =========================

        private static OpenFoodFactsProduct? TryDeserialize(string jsonLine)
        {
            try { return JsonSerializer.Deserialize<OpenFoodFactsProduct>(jsonLine, JsonOpts); }
            catch { return null; }
        }

        private static Product MapToProduct(OpenFoodFactsProduct src)
        {
            var n = src.OpenFoodFactsNutriments;

            return new Product
            {
                Code            = Sanitizer(src.Code)!,
                ProductName     = Sanitizer(src.ProductName),

                BrandOwner      = Sanitizer(src.BrandOwner),
                Brands          = Sanitizer(src.Brands),

                Language        = Sanitizer(src.Language),
                LanguageCode    = Sanitizer(src.LanguageCode),
                IngredientsText = Sanitizer(src.IngredientsText),

                NutritionGrade  = Sanitizer(src.NutritionGrade),
                NovaGroup       = src.NovaGroup,
                EcoScoreGrade   = Sanitizer(src.EcoScoreGrade),
                ServingSize     = Sanitizer(src.ServingSize),
                IsVegetarian    = Sanitizer(src.IsVegetarian),
                IsVegan         = Sanitizer(src.IsVegan),

                Energy100g          = n?.Energy100g,
                EnergyKcal100g      = n?.EnergyKcal100g,
                Fat100g             = n?.Fat100g,
                SaturatedFat100g    = n?.SaturatedFat100g,
                Carbohydrates100g   = n?.Carbohydrates100g,
                Sugars100g          = n?.Sugars100g,
                Fiber100g           = n?.Fiber100g,
                Proteins100g        = n?.Proteins100g,
                Salt100g            = n?.Salt100g,
                Sodium100g          = n?.Sodium100g,
                EnergyKcalServing   = n?.EnergyKcalServing,

                LastUpdated = ConvertUnixToDateTime(src.LastUpdatedT)
            };
        }

        // ========================= Kolekcjonowanie tagów =========================

        private static void CollectTags(OpenFoodFactsProduct src, string code, TagBuffers tags)
        {
            foreach (var t in src.CategoriesTags ?? Enumerable.Empty<string>())
                tags.AddCategory(code, t);

            foreach (var t in src.CountriesTags ?? Enumerable.Empty<string>())
                tags.AddCountry(code, t);

            foreach (var t in src.AllergensTags ?? Enumerable.Empty<string>())
                tags.AddAllergen(code, t);

            foreach (var t in src.IngredientsTags ?? Enumerable.Empty<string>())
                tags.AddIngredient(code, t);
        }

        

        private async Task FlushAsync(List<Product> products, TagBuffers tags, CancellationToken ct)
        {
            await _repo.BulkInsertProductsAsync(products, ct);

            await _repo.BulkEnsureTagsAsync<IngredientTag>(tags.IngredientNames, ct);
            await _repo.BulkEnsureTagsAsync<CountryTag>(tags.CountryNames, ct);
            await _repo.BulkEnsureTagsAsync<CategoryTag>(tags.CategoryNames, ct);
            await _repo.BulkEnsureTagsAsync<AllergenTag>(tags.AllergenNames, ct);

            await _repo.BulkUpsertProductIngredientLinksAsync(tags.IngredientLinks, ct);
            await _repo.BulkUpsertProductCountryLinksAsync(tags.CountryLinks, ct);
            await _repo.BulkUpsertProductCategoryLinksAsync(tags.CategoryLinks, ct);
            await _repo.BulkUpsertProductAllergenLinksAsync(tags.AllergenLinks, ct);
        }

        private static async IAsyncEnumerable<string> ReadLinesAsync(string filePath, [EnumeratorCancellation] CancellationToken ct = default)
        {
            using var fs = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 1_048_576,
                useAsync: true);

            using var reader = new StreamReader(fs);

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                ct.ThrowIfCancellationRequested();
                if (line.IndexOf('\0') >= 0) line = line.Replace("\0", string.Empty);
                yield return line;
            }
        }
        
        
        private static string? Sanitizer(string? s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            var cleaned = new string(s.Where(ch => ch != '\0' && ch >= ' ' || ch is '\t' or '\n' or '\r').ToArray());
            cleaned = cleaned.Trim();
            return cleaned.Length == 0 ? null : cleaned;
        }

        
        private static DateTime? ConvertUnixToDateTime(long? unix)
        {
            if (unix is null) return null;
            try
            {
                var val = unix.Value;
                if (val >= 1_000_000_000_000L) val /= 1000; // ms → s
                return DateTimeOffset.FromUnixTimeSeconds(val).UtcDateTime;
            }
            catch
            {
                return null;
            }
        }


        private sealed class TagBuffers
        {
            public HashSet<string> IngredientNames { get; }
            public HashSet<string> CountryNames { get; }
            public HashSet<string> CategoryNames { get; }
            public HashSet<string> AllergenNames { get; }

            public List<(string Code, string TagName)> IngredientLinks { get; }
            public List<(string Code, string TagName)> CountryLinks { get; }
            public List<(string Code, string TagName)> CategoryLinks { get; }
            public List<(string Code, string TagName)> AllergenLinks { get; }

            public TagBuffers(int batchCapacity)
            {
                IngredientNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                CountryNames    = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                CategoryNames   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                AllergenNames   = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                IngredientLinks = new List<(string, string)>(batchCapacity * 3);
                CountryLinks    = new List<(string, string)>(batchCapacity * 1);
                CategoryLinks   = new List<(string, string)>(batchCapacity * 2);
                AllergenLinks   = new List<(string, string)>(batchCapacity * 1);
            }

            public void AddIngredient(string code, string tag)
            {
                IngredientNames.Add(tag);
                IngredientLinks.Add((code, tag));
            }

            public void AddCountry(string code, string tag)
            {
                CountryNames.Add(tag);
                CountryLinks.Add((code, tag));
            }

            public void AddCategory(string code, string tag)
            {
                CategoryNames.Add(tag);
                CategoryLinks.Add((code, tag));
            }

            public void AddAllergen(string code, string tag)
            {
                AllergenNames.Add(tag);
                AllergenLinks.Add((code, tag));
            }

            public void Clear()
            {
                IngredientNames.Clear();
                CountryNames.Clear();
                CategoryNames.Clear();
                AllergenNames.Clear();

                IngredientLinks.Clear();
                CountryLinks.Clear();
                CategoryLinks.Clear();
                AllergenLinks.Clear();
            }
        }
    }
}
