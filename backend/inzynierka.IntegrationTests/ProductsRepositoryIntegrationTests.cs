using inzynierka.Products.Model;
using inzynierka.Products.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace inzynierka.IntegrationTests;

public class ProductsRepositoryIntegrationTests : DatabaseIntegrationTest
{
    private ProductRepository _repository = null!;

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var logger = ServiceProvider.GetRequiredService<ILogger<ProductRepository>>();
        _repository = new ProductRepository(DbContext, logger);
    }

    [Fact]
    public async Task AddProduct_ShouldAddProductToDatabase()
    {
        // Arrange
        var product = new Product 
        { 
            Code = "123456789", 
            ProductName = "Test Product",
            Language = "pl"
        };

        // Act
        await _repository.AddProductAsync(product);
        var result = await _repository.GetProductByCodeAsync("123456789");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Product", result.ProductName);
        Assert.Equal("123456789", result.Code);
    }

    [Fact]
    public async Task AddProductAsync_ShouldAddProduct()
    {
        // Arrange
        var product = new Product
        {
            Code = "123456",
            ProductName = "Test Product",
            Language = "pl",
            BrandOwner = "Test Brand"
        };

        // Act
        var result = await _repository.AddProductAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("123456", result.Code);
        Assert.Equal("Test Product", result.ProductName);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Act
        var result = await _repository.GetProductByIdAsync(99999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetProductByCodeAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Act
        var result = await _repository.GetProductByCodeAsync("nonexistent");

        // Assert
        Assert.Null(result);
    }
    
    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct()
    {
        // Arrange
        var product = new Product
        {
            Code = "555666",
            ProductName = "Original Name",
            Language = "pl"
        };
        await _repository.AddProductAsync(product);

        // Act
        product.ProductName = "Updated Name";
        product.BrandOwner = "New Brand";
        var result = await _repository.UpdateProductAsync(product);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Updated Name", result.ProductName);
        Assert.Equal("New Brand", result.BrandOwner);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct()
    {
        // Arrange
        var product = new Product
        {
            Code = "777888",
            ProductName = "Product to Delete",
            Language = "pl"
        };
        await _repository.AddProductAsync(product);
        var productId = product.Id;

        // Act
        await _repository.DeleteProductAsync(productId);
        var result = await _repository.GetProductByIdAsync(productId);

        // Assert
        Assert.Null(result);
    }
    
}

