using System.Text;
using inzynierka.IO.Pipeline;
using inzynierka.Products.Model;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;
using inzynierka.Products.Repositories;

namespace inzynierka.Products.OpenFoodFacts.Import
{
    /// <summary>
    /// Odbiornik paczek dla potoku ETL: mapuje surowe rekordy OpenFoodFacts na encje
    /// <see cref="Product"/>, zbiera linki tagów i oddaje całą paczkę do masowego zapisu
    /// w jednej transakcji (<see cref="IProductBulkRepository.BulkImportBatchAsync"/>).
    ///
    /// Silnik woła <see cref="WriteBatchAsync"/> sekwencyjnie, więc liczniki nie wymagają synchronizacji.
    /// </summary>
    public sealed class ProductBulkSink : IBatchSink<OpenFoodFactsProduct>
    {
        private readonly IProductBulkRepository _bulkRepository;

        public long Imported { get; private set; }
        public long SkippedNoCode { get; private set; }
        public long SkippedNoNutrition { get; private set; }

        public ProductBulkSink(IProductBulkRepository bulkRepository) => _bulkRepository = bulkRepository;

        public async ValueTask WriteBatchAsync(IReadOnlyList<OpenFoodFactsProduct> batch, CancellationToken cancellationToken)
        {
            var payload = new ProductBatch(batch.Count);

            foreach (var src in batch)
            {
                var code = Sanitizer(src.Code);
                if (string.IsNullOrWhiteSpace(code)) { SkippedNoCode++; continue; }
                code = code.Trim();

                var product = MapToProduct(src);
                if (product is null) { SkippedNoNutrition++; continue; }

                payload.Products.Add(product);
                CollectTags(src, code, payload);
            }

            if (payload.Products.Count == 0)
                return;

            await _bulkRepository.BulkImportBatchAsync(payload, cancellationToken);
            Imported += payload.Products.Count;
        }

        private static void CollectTags(OpenFoodFactsProduct src, string code, ProductBatch batch)
        {
            foreach (var t in src.CategoriesTags ?? Enumerable.Empty<string>())
                batch.CategoryLinks.Add((code, t));

            foreach (var t in src.CountriesTags ?? Enumerable.Empty<string>())
                batch.CountryLinks.Add((code, t));

            foreach (var t in src.AllergensTags ?? Enumerable.Empty<string>())
                batch.AllergenLinks.Add((code, t));

            foreach (var t in src.IngredientsTags ?? Enumerable.Empty<string>())
                batch.IngredientLinks.Add((code, t));
        }

        private static Product? MapToProduct(OpenFoodFactsProduct src)
        {
            var n = src.OpenFoodFactsNutriments;
            if (n?.Carbohydrates100g is null || n.Fat100g is null || n.Proteins100g is null || n.EnergyKcal100g is null)
                return null;

            return new Product
            {
                Code = Sanitizer(src.Code) ?? string.Empty,
                ProductName = Sanitizer(src.ProductName),

                BrandOwner = Sanitizer(src.BrandOwner),
                Brands = Sanitizer(src.Brands),

                Language = Sanitizer(src.Language),
                LanguageCode = Sanitizer(src.LanguageCode),
                IngredientsText = Sanitizer(src.IngredientsText),

                NutritionGrade = Sanitizer(src.NutritionGrade),
                NovaGroup = src.NovaGroup,
                EcoScoreGrade = Sanitizer(src.EcoScoreGrade),
                ServingSize = Sanitizer(src.ServingSize),
                IsVegetarian = Sanitizer(src.IsVegetarian),
                IsVegan = Sanitizer(src.IsVegan),

                Energy100g = n.Energy100g,
                EnergyKcal100g = n.EnergyKcal100g,
                Fat100g = n.Fat100g,
                SaturatedFat100g = n.SaturatedFat100g,
                Carbohydrates100g = n.Carbohydrates100g,
                Sugars100g = n.Sugars100g,
                Fiber100g = n.Fiber100g,
                Proteins100g = n.Proteins100g,
                Salt100g = n.Salt100g,
                Sodium100g = n.Sodium100g,
                EnergyKcalServing = n.EnergyKcalServing,

                LastUpdated = ConvertUnixToDateTime(src.LastUpdatedT)
            };
        }

        private static string? Sanitizer(string? s)
        {
            if (string.IsNullOrEmpty(s)) return s;

            var sb = new StringBuilder(s.Length);
            foreach (var ch in s)
                if (ch != '\0' && (ch >= ' ' || ch is '\t' or '\n' or '\r'))
                    sb.Append(ch);

            var start = 0;
            var end = sb.Length - 1;
            while (start <= end && sb[start] is ' ' or '\t' or '\n' or '\r') start++;
            while (end >= start && sb[end] is ' ' or '\t' or '\n' or '\r') end--;

            if (start > end) return null;
            return sb.ToString(start, end - start + 1);
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
    }
}
