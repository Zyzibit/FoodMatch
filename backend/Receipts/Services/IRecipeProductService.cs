using inzynierka.AI.Contracts.Models;
using inzynierka.Products.Model;

namespace inzynierka.Receipts.Services;

/// <summary>
/// Service for managing products within recipe context.
/// Handles creation of AI-generated products for recipes.
/// </summary>
public interface IRecipeProductService
{
    /// <summary>
    /// Creates an AI-generated product from a recipe ingredient.
    /// </summary>
    Task<Product> CreateAiGeneratedProductAsync(GeneratedRecipeIngredient ingredient);
}

