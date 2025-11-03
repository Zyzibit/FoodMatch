using inzynierka.Products.Model;
using inzynierka.Receipts.Model.Recipe;

namespace inzynierka.Receipts.Services;

public interface IRecipeProductService
{
    Task<Product> CreateAiGeneratedProductAsync(GeneratedRecipeIngredient ingredient);
}
