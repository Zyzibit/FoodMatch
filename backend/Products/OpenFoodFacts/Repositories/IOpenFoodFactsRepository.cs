using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.CountryTag;
using inzynierka.Products.Model.Tag.IngredientTag;
using System.Collections.Concurrent;

namespace inzynierka.Products.OpenFoodFacts.Repositories;

/// <summary>
/// Repozytorium obsługujące import danych OpenFoodFacts.
/// Zawiera operacje na produktach, tagach i pamięci podręcznej.
/// </summary>
public interface IOpenFoodFactsRepository
{
    // ----------------------------
    // 🟢 Produkty
    // ----------------------------

    /// <summary>
    /// Wstawia produkty w trybie batch, bez IncludeGraph.
    /// </summary>
    Task BulkInsertProductsAsync(IEnumerable<Product> products);

    /// <summary>
    /// Zwraca zbiór kodów produktów istniejących w bazie spośród podanej listy.
    /// </summary>
    Task<HashSet<string>> GetExistingProductCodesBatchAsync(IEnumerable<string> codes);

    /// <summary>
    /// Sprawdza, czy produkt o danym kodzie istnieje (wolniejsze, do wyjątkowych przypadków).
    /// </summary>
    Task<bool> ProductExistsAsync(string productCode);


    // ----------------------------
    // 🟣 Tagi
    // ----------------------------

    /// <summary>
    /// Tworzy brakujące tagi w bazie i zwraca pełną listę utworzonych.
    /// </summary>
    Task<List<T>> CreateTagsAsync<T>(IEnumerable<string> tagNames)
        where T : class, ITagEntity, new();

    /// <summary>
    /// Ładuje wszystkie istniejące tagi danego typu z bazy (np. dla synchronizacji).
    /// </summary>
    Task<IEnumerable<T>> GetAllTagsAsync<T>()
        where T : class, ITagEntity;

    /// <summary>
    /// Pobiera tag o konkretnej nazwie (lub null, jeśli nie istnieje).
    /// </summary>
    Task<T?> GetTagByNameAsync<T>(string name)
        where T : class, ITagEntity;


    // ----------------------------
    // 🧠 Cache tagów (Redis)
    // ----------------------------

    /// <summary>
    /// Wczytuje cache tagów z Redis lub — jeśli brak — z bazy (słownik name → id).
    /// </summary>
    Task<Dictionary<string, int>> LoadTagCacheIdsAsync<T>()
        where T : class, ITagEntity;

    /// <summary>
    /// Zapisuje cache tagów do Redis (słownik name → id).
    /// </summary>
    Task SaveTagCacheIdsAsync<T>(ConcurrentDictionary<string, int> cache)
        where T : class, ITagEntity;
}
