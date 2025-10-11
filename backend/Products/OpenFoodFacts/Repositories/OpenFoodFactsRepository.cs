using EFCore.BulkExtensions;
using inzynierka.Data;
using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag;
using inzynierka.Products.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace inzynierka.Products.OpenFoodFacts.Repositories;

public class OpenFoodFactsRepository : IOpenFoodFactsRepository
{
    private readonly AppDbContext _context;
    private readonly IRedisCacheService _cache;
    private readonly ILogger<OpenFoodFactsRepository> _logger;

    private const int BULK_BATCH_SIZE = 2000;
    private static readonly TimeSpan CACHE_EXPIRY = TimeSpan.FromHours(24);

    public OpenFoodFactsRepository(AppDbContext context, IRedisCacheService cache, ILogger<OpenFoodFactsRepository> logger)
    {
        _context = context;
        _cache = cache;
        _logger = logger;
    }



    public async Task BulkInsertProductsAsync(IEnumerable<Product> products)
    {
        var list = products.ToList();
        if (list.Count == 0) return;

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1️⃣ Wstaw produkty i pobierz ich Id
            await _context.BulkInsertAsync(list, new BulkConfig
            {
                PreserveInsertOrder = true,
                SetOutputIdentity = true,
                BatchSize = BULK_BATCH_SIZE,
                UseTempDB = true,
                IncludeGraph = false
            });

            // 2️⃣ Ustaw ProductId w relacjach (teraz Id produktów już są)
            foreach (var product in list)
            {
                if (product.Id <= 0)
                    continue;

                if (product.ProductCountryTags != null)
                    foreach (var r in product.ProductCountryTags)
                        r.ProductId = product.Id;

                if (product.ProductCategoryTags != null)
                    foreach (var r in product.ProductCategoryTags)
                        r.ProductId = product.Id;

                if (product.ProductAllergenTags != null)
                    foreach (var r in product.ProductAllergenTags)
                        r.ProductId = product.Id;

                if (product.ProductIngredientTags != null)
                    foreach (var r in product.ProductIngredientTags)
                        r.ProductId = product.Id;
            }

            // 3️⃣ Wstaw relacje po przypisaniu ProductId
            await _context.BulkInsertAsync(list.SelectMany(p => p.ProductCountryTags), new BulkConfig
            {
                BatchSize = BULK_BATCH_SIZE,
                UseTempDB = true
            });

            await _context.BulkInsertAsync(list.SelectMany(p => p.ProductCategoryTags), new BulkConfig
            {
                BatchSize = BULK_BATCH_SIZE,
                UseTempDB = true
            });

            await _context.BulkInsertAsync(list.SelectMany(p => p.ProductAllergenTags), new BulkConfig
            {
                BatchSize = BULK_BATCH_SIZE,
                UseTempDB = true
            });

            await _context.BulkInsertAsync(list.SelectMany(p => p.ProductIngredientTags), new BulkConfig
            {
                BatchSize = BULK_BATCH_SIZE,
                UseTempDB = true
            });

            await transaction.CommitAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Bulk insert failed for {Count} products", list.Count);
            throw;
        }
    }



    public async Task<HashSet<string>> GetExistingProductCodesBatchAsync(IEnumerable<string> codes)
    {
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var codeList = codes.ToList();

        if (codeList.Count == 0) return set;

        var existing = await _context.Products
            .AsNoTracking()
            .Where(p => codeList.Contains(p.Code))
            .Select(p => p.Code)
            .ToListAsync();

        foreach (var c in existing)
            set.Add(c);

        return set;
    }

    public async Task<bool> ProductExistsAsync(string productCode)
    {
        return await _context.Products.AsNoTracking().AnyAsync(p => p.Code == productCode);
    }

    // 🔹 Tagi

    public async Task<List<T>> CreateTagsAsync<T>(IEnumerable<string> tagNames) where T : class, ITagEntity, new()
    {
        var clean = tagNames
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (clean.Count == 0) return [];

        var newTags = clean.Select(n => new T { Name = n }).ToList();
        await _context.Set<T>().AddRangeAsync(newTags);
        await _context.SaveChangesAsync();
        return newTags;
    }

    public async Task<IEnumerable<T>> GetAllTagsAsync<T>() where T : class, ITagEntity
    {
        return await _context.Set<T>().AsNoTracking().ToListAsync();
    }

    public async Task<T?> GetTagByNameAsync<T>(string name) where T : class, ITagEntity
    {
        return await _context.Set<T>().AsNoTracking().FirstOrDefaultAsync(t => t.Name == name);
    }



    public async Task<Dictionary<string, int>> LoadTagCacheIdsAsync<T>() where T : class, ITagEntity
    {
        var key = GetCacheKey<T>();
        var cached = await _cache.GetAsync<Dictionary<string, int>>(key);
        if (cached != null) return cached;

        var tags = await _context.Set<T>()
            .AsNoTracking()
            .Select(t => new { t.Name, t.Id })
            .ToListAsync();

        var data = tags
            .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .ToDictionary(t => t.Name, t => t.Id, StringComparer.OrdinalIgnoreCase);

        await _cache.SetAsync(key, data, CACHE_EXPIRY);
        return data;
    }
    public async Task SaveTagCacheIdsAsync<T>(ConcurrentDictionary<string, int> cache) where T : class, ITagEntity
    {
        var key = GetCacheKey<T>();
        await _cache.SetAsync(key, cache.ToDictionary(k => k.Key, v => v.Value), CACHE_EXPIRY);
    }

    private static string GetCacheKey<T>() where T : class, ITagEntity
    {
        return typeof(T).Name switch
        {
            "CountryTag" => "import:country_tags_ids",
            "CategoryTag" => "import:category_tags_ids",
            "AllergenTag" => "import:allergen_tags_ids",
            "IngredientTag" => "import:ingredient_tags_ids",
            _ => $"import:{typeof(T).Name.ToLowerInvariant()}_tags_ids"
        };
    }
}
