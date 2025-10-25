using inzynierka.AI.Contracts.Models;
using inzynierka.Products.Model;
using inzynierka.Products.Contracts;
using inzynierka.Products.Repositories;

namespace inzynierka.Receipts.Services;

public class RecipeProductService : IRecipeProductService
{
    private readonly ILogger<RecipeProductService> _logger;
    private readonly IProductContract _productContract;
    private readonly IProductRepository _productRepository;

    public RecipeProductService(
        ILogger<RecipeProductService> logger,
        IProductContract productContract,
        IProductRepository productRepository)
    {
        _logger = logger;
        _productContract = productContract;
        _productRepository = productRepository;
    }
    
    public async Task<Product> CreateAiGeneratedProductAsync(GeneratedRecipeIngredient ingredient)
    {
        if (string.IsNullOrEmpty(ingredient.Name))
        {
            throw new ArgumentException("Ingredient name cannot be null or empty", nameof(ingredient));
        }

        try
        {
            var result = await _productContract.AddAiProductAsync(ingredient.Name);
            
            if (!result.Success || result.Product == null)
            {
                throw new InvalidOperationException($"Failed to create AI product: {result.ErrorMessage}");
            }

            var productId = int.Parse(result.Product.Id);
            var createdProduct = await _productRepository.GetProductByIdAsync(productId);

            if (createdProduct == null)
            {
                throw new InvalidOperationException($"Created product with ID {productId} could not be retrieved");
            }

            _logger.LogInformation("Created AI-generated product: {ProductName} with ID: {ProductId}", 
                ingredient.Name, createdProduct.Id);
            
            return createdProduct;
        }
        catch (Exception ex) when (ex is not ArgumentException && ex is not InvalidOperationException)
        {
            _logger.LogError(ex, "Error creating AI-generated product: {ProductName}", ingredient.Name);
            throw new InvalidOperationException($"Failed to create product for ingredient '{ingredient.Name}'", ex);
        }
    }
}
