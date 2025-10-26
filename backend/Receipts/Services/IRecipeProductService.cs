using inzynierka.AI.Contracts.Models;
using inzynierka.Products.Model;

namespace inzynierka.Receipts.Services;

public interface IRecipeProductService
{
    Task<Product> CreateAiGeneratedProductAsync(GeneratedRecipeIngredient ingredient);
}
