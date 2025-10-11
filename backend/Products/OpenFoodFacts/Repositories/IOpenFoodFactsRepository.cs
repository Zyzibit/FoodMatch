using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag;

namespace inzynierka.Products.OpenFoodFacts.Repositories
{
    public interface IOpenFoodFactsRepository
    {
        void PrepareForBulkImport();
        void RestoreAfterBulkImport();

        Task<Dictionary<string, int>> GetTagDictionaryAsync<T>(CancellationToken ct = default) where T : class, ITagEntity;
        Task BulkEnsureTagsAsync<T>(IReadOnlyCollection<string> tagNames, CancellationToken ct = default)
            where T : class, ITagEntity, new();
        Task BulkUpsertProductIngredientLinksAsync(List<(string Code, string TagName)> items, CancellationToken ct = default);
        Task BulkUpsertProductCountryLinksAsync(List<(string Code, string TagName)> items, CancellationToken ct = default);
        Task BulkUpsertProductCategoryLinksAsync(List<(string Code, string TagName)> items, CancellationToken ct = default);
        Task BulkUpsertProductAllergenLinksAsync(List<(string Code, string TagName)> items, CancellationToken ct = default);

        Task BulkInsertProductsAsync(List<Product> products, CancellationToken ct = default);
        Task BulkInsertJoinsAsync<TJoin>(List<TJoin> joins, CancellationToken ct = default) where TJoin : class;
    }
}
