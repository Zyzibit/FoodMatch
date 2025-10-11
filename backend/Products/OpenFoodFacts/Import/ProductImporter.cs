using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.CountryTag;
using inzynierka.Products.Model.Tag.IngredientTag;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Models;
using inzynierka.Products.OpenFoodFacts.OpenFoodFactsDeserializer.Services;
using inzynierka.Products.OpenFoodFacts.Repositories;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Runtime;

namespace inzynierka.Products.OpenFoodFacts.Import;

public class ProductImporter : IProductImporter
{
    private readonly IOpenFoodFactsDeserializer _deserializer;
    private readonly IOpenFoodFactsRepository _repository;
    private readonly ILogger<ProductImporter> _logger;

    private readonly ConcurrentDictionary<string, int> _countryTags = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> _categoryTags = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> _allergenTags = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<string, int> _ingredientTags = new(StringComparer.OrdinalIgnoreCase);

    public ProductImporter(
        IOpenFoodFactsDeserializer deserializer,
        IOpenFoodFactsRepository repository,
        ILogger<ProductImporter> logger)
    {
        _deserializer = deserializer;
        _repository = repository;
        _logger = logger;
    }

    public async Task<ImportResult> ImportAsync(string path, int maxProducts, int batchSize)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Import file not found: {path}", path);

        GCSettings.LatencyMode = GCLatencyMode.Batch;

        int imported = 0, skipped = 0;
        var batch = new List<(Product product, OpenFoodFactsProduct json)>(batchSize);

        _logger.LogInformation("Initializing tag caches...");
        await InitializeTagCachesAsync();

        await foreach (var json in _deserializer.DeserializeFromJsonlFileAsync(path))
        {
            if (imported + skipped >= maxProducts)
                break;

            var product = Map(json);
            if (string.IsNullOrWhiteSpace(product.Code))
            {
                skipped++;
                continue;
            }

            batch.Add((product, json));

            if (batch.Count >= batchSize)
            {
                skipped += await FilterExistingProductsAsync(batch);
                await ProcessBatchAsync(batch);
                imported += batch.Count;
                batch.Clear();
                _logger.LogInformation("Imported {Count} (total: {Total})", batchSize, imported);
            }
        }

        if (batch.Count > 0)
        {
            skipped += await FilterExistingProductsAsync(batch);
            await ProcessBatchAsync(batch);
            imported += batch.Count;
        }

        _ = Task.Run(() => SaveTagCachesAsync());

        return new ImportResult
        {
            ImportedCount = imported,
            SkippedCount = skipped
        };
    }

    private async Task ProcessBatchAsync(List<(Product product, OpenFoodFactsProduct json)> batch)
    {
        foreach (var (product, json) in batch)
        {
            var tags = await GetAllTagsAsync(json);
            SetupProductRelationships(product, tags);
        }

        await _repository.BulkInsertProductsAsync(batch.Select(b => b.product).ToList());
    }

    private async Task<int> FilterExistingProductsAsync(List<(Product product, OpenFoodFactsProduct json)> batch)
    {
        var codes = batch.Select(b => b.product.Code).Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        var existing = await _repository.GetExistingProductCodesBatchAsync(codes);
        batch.RemoveAll(p => existing.Contains(p.product.Code, StringComparer.OrdinalIgnoreCase));
        return existing.Count;
    }

    private async Task InitializeTagCachesAsync()
    {
        var country = await _repository.LoadTagCacheIdsAsync<CountryTag>();
        foreach (var kv in country) _countryTags[kv.Key] = kv.Value;

        var category = await _repository.LoadTagCacheIdsAsync<CategoryTag>();
        foreach (var kv in category) _categoryTags[kv.Key] = kv.Value;

        var allergen = await _repository.LoadTagCacheIdsAsync<AllergenTag>();
        foreach (var kv in allergen) _allergenTags[kv.Key] = kv.Value;

        var ingredient = await _repository.LoadTagCacheIdsAsync<IngredientTag>();
        foreach (var kv in ingredient) _ingredientTags[kv.Key] = kv.Value;
    }

    private async Task SaveTagCachesAsync()
    {
        await _repository.SaveTagCacheIdsAsync<CountryTag>(_countryTags);
        await _repository.SaveTagCacheIdsAsync<CategoryTag>(_categoryTags);
        await _repository.SaveTagCacheIdsAsync<AllergenTag>(_allergenTags);
        await _repository.SaveTagCacheIdsAsync<IngredientTag>(_ingredientTags);
        _logger.LogInformation("Tag caches saved.");
    }

    private static Product Map(OpenFoodFactsProduct src)
    {
        return new Product
        {
            Code = src.Code,
            Language = src.Language,
            BrandOwner = src.BrandOwner,
            ProductName = src.ProductName,
            Brands = src.Brands,
            NutritionGrade = src.NutritionGrade,
            NovaGroup = src.NovaGroup,
            EcoScoreGrade = src.EcoScoreGrade,
            IngredientsText = CleanNullBytes(src.IngredientsText),
            ServingSize = src.ServingSize,
            IsVegetarian = src.IsVegetarian,
            IsVegan = src.IsVegan,
            LastUpdated = src.LastUpdatedT > 0 ? DateTimeOffset.FromUnixTimeSeconds(src.LastUpdatedT).UtcDateTime : null,
            Energy100g = src.OpenFoodFactsNutriments?.Energy100g ?? 0,
            EnergyKcal100g = src.OpenFoodFactsNutriments?.EnergyKcal100g ?? 0,
            Fat100g = src.OpenFoodFactsNutriments?.Fat100g ?? 0,
            SaturatedFat100g = src.OpenFoodFactsNutriments?.SaturatedFat100g ?? 0,
            Carbohydrates100g = src.OpenFoodFactsNutriments?.Carbohydrates100g ?? 0,
            Sugars100g = src.OpenFoodFactsNutriments?.Sugars100g ?? 0,
            Fiber100g = src.OpenFoodFactsNutriments?.Fiber100g ?? 0,
            Proteins100g = src.OpenFoodFactsNutriments?.Proteins100g ?? 0,
            Salt100g = src.OpenFoodFactsNutriments?.Salt100g ?? 0,
            Sodium100g = src.OpenFoodFactsNutriments?.Sodium100g ?? 0,
            EnergyKcalServing = src.OpenFoodFactsNutriments?.EnergyKcalServing ?? 0,
        };
    }

    private async Task<(List<CountryTag>, List<CategoryTag>, List<AllergenTag>, List<IngredientTag>)>
        GetAllTagsAsync(OpenFoodFactsProduct json)
    {
        var countries = await GetOrCreateTagsAsync<CountryTag>(_countryTags, json.CountriesTags);
        var categories = await GetOrCreateTagsAsync<CategoryTag>(_categoryTags, json.CategoriesTags);
        var allergens = await GetOrCreateTagsAsync<AllergenTag>(_allergenTags, json.AllergensTags);
        var ingredients = await GetOrCreateTagsAsync<IngredientTag>(_ingredientTags, json.IngredientsTags);
        return (countries, categories, allergens, ingredients);
    }

    private async Task<List<T>> GetOrCreateTagsAsync<T>(
        ConcurrentDictionary<string, int> cache, List<string>? names)
        where T : class, ITagEntity, new()
    {
        if (names == null || names.Count == 0) return [];

        names = names
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var found = new List<T>();
        var missing = new List<string>();

        foreach (var n in names)
        {
            if (cache.TryGetValue(n, out var id))
                found.Add(new T { Id = id, Name = n });
            else
                missing.Add(n);
        }

        if (missing.Count == 0)
            return found;

        var newTags = await _repository.CreateTagsAsync<T>(missing);
        foreach (var tag in newTags)
        {
            cache[tag.Name] = tag.Id;
            found.Add(tag);
        }

        return found;
    }

    private static void SetupProductRelationships(Product p,
        (List<CountryTag> countries, List<CategoryTag> categories, List<AllergenTag> allergens, List<IngredientTag> ingredients) tags)
    {
        p.ProductCountryTags = tags.countries.Select(t => new ProductCountryTag { CountryTagId = t.Id }).ToList();
        p.ProductCategoryTags = tags.categories.Select(t => new ProductCategoryTag { CategoryTagId = t.Id }).ToList();
        p.ProductAllergenTags = tags.allergens.Select(t => new ProductAllergenTag { AllergenTagId = t.Id }).ToList();
        p.ProductIngredientTags = tags.ingredients.Select(t => new ProductIngredientTag { IngredientTagId = t.Id }).ToList();
    }

    private static string? CleanNullBytes(string? v)
    {
        if (string.IsNullOrEmpty(v) || !v.Contains('\0'))
            return v;

        Span<char> buf = stackalloc char[v.Length];
        int j = 0;
        foreach (var c in v)
            if (c != '\0') buf[j++] = c;
        return new string(buf[..j]);
    }
}
