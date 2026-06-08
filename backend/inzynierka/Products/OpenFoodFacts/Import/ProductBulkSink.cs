using System.Text;
using inzynierka.IO.Pipeline;
using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.CountryTag;
using inzynierka.Products.Model.Tag.IngredientTag;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;
using inzynierka.Products.Repositories;

namespace inzynierka.Products.OpenFoodFacts.Import
{
    /// <summary>
    /// Odbiornik paczek dla potoku ETL: mapuje surowe rekordy OpenFoodFacts na encje
    /// <see cref="Product"/>, zbiera tagi i wykonuje masowy zapis przez
    /// <see cref="IProductBulkRepository"/> (PostgreSQL binary COPY + MERGE).
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
            var products = new List<Product>(batch.Count);
            var tags = new TagBuffers(batch.Count);

            foreach (var src in batch)
            {
                var code = Sanitizer(src.Code);
                if (string.IsNullOrWhiteSpace(code)) { SkippedNoCode++; continue; }
                code = code.Trim();

                var product = MapToProduct(src);
                if (product is null) { SkippedNoNutrition++; continue; }

                products.Add(product);
                CollectTags(src, code, tags);
            }

            if (products.Count == 0)
                return;

            await FlushAsync(products, tags, cancellationToken);
            Imported += products.Count;
        }

        private async Task FlushAsync(List<Product> products, TagBuffers tags, CancellationToken ct)
        {
            await _bulkRepository.BulkInsertProductsAsync(products, ct);

            await _bulkRepository.BulkEnsureTagsAsync<IngredientTag>(tags.IngredientNames, ct);
            await _bulkRepository.BulkEnsureTagsAsync<CountryTag>(tags.CountryNames, ct);
            await _bulkRepository.BulkEnsureTagsAsync<CategoryTag>(tags.CategoryNames, ct);
            await _bulkRepository.BulkEnsureTagsAsync<AllergenTag>(tags.AllergenNames, ct);

            await _bulkRepository.BulkUpsertProductIngredientLinksAsync(tags.IngredientLinks, ct);
            await _bulkRepository.BulkUpsertProductCountryLinksAsync(tags.CountryLinks, ct);
            await _bulkRepository.BulkUpsertProductCategoryLinksAsync(tags.CategoryLinks, ct);
            await _bulkRepository.BulkUpsertProductAllergenLinksAsync(tags.AllergenLinks, ct);
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

            public void AddIngredient(string code, string tag) { IngredientNames.Add(tag); IngredientLinks.Add((code, tag)); }
            public void AddCountry(string code, string tag)    { CountryNames.Add(tag);    CountryLinks.Add((code, tag)); }
            public void AddCategory(string code, string tag)   { CategoryNames.Add(tag);   CategoryLinks.Add((code, tag)); }
            public void AddAllergen(string code, string tag)   { AllergenNames.Add(tag);   AllergenLinks.Add((code, tag)); }
        }
    }
}
