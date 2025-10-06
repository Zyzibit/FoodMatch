using Grpc.Core;
using inzynierka.Data;
using inzynierka.Products.Grpc;
using inzynierka.Products.OpenFoodFacts.Import;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Products.Grpc.Services;

public class ProductsGrpcService : ProductService.ProductServiceBase
{
    private readonly AppDbContext _context;
    private readonly IProductImporter _productImporter;
    private readonly ILogger<ProductsGrpcService> _logger;

    public ProductsGrpcService(
        AppDbContext context,
        IProductImporter productImporter,
        ILogger<ProductsGrpcService> logger)
    {
        _context = context;
        _productImporter = productImporter;
        _logger = logger;
    }

    public override async Task<GetProductResponse> GetProduct(
        GetProductRequest request, 
        ServerCallContext context)
    {
        try
        {
            var product = await _context.Products
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .FirstOrDefaultAsync(p => p.Id.ToString() == request.ProductId);

            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "Product not found"));
            }

            var grpcProduct = new Product
            {
                Id = product.Id.ToString(),
                Name = product.ProductName ?? "",
                Brand = product.Brands ?? "",
                Barcode = product.Code ?? "",
                ImageUrl = product.ImageUrl ?? ""
            };

            grpcProduct.Categories.AddRange(
                product.ProductCategoryTags.Select(pct => pct.CategoryTag.Name));
            grpcProduct.Ingredients.AddRange(
                product.ProductIngredientTags.Select(pit => pit.IngredientTag.Name));
            grpcProduct.Allergens.AddRange(
                product.ProductAllergenTags.Select(pat => pat.AllergenTag.Name));
            grpcProduct.Countries.AddRange(
                product.ProductCountryTags.Select(pct => pct.CountryTag.Name));

            return new GetProductResponse { Product = grpcProduct };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product with ID: {ProductId}", request.ProductId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<SearchProductsResponse> SearchProducts(
        SearchProductsRequest request, 
        ServerCallContext context)
    {
        try
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrEmpty(request.Query))
            {
                query = query.Where(p => p.ProductName!.Contains(request.Query) ||
                                        p.Brands!.Contains(request.Query));
            }

            var totalCount = await query.CountAsync();
            
            var products = await query
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .Skip(request.Offset)
                .Take(request.Limit)
                .ToListAsync();

            var grpcProducts = products.Select(p => new Product
            {
                Id = p.Id.ToString(),
                Name = p.ProductName ?? "",
                Brand = p.Brands ?? "",
                Barcode = p.Code ?? "",
                ImageUrl = p.ImageUrl ?? "",
                Categories = { p.ProductCategoryTags.Select(pct => pct.CategoryTag.Name) },
                Ingredients = { p.ProductIngredientTags.Select(pit => pit.IngredientTag.Name) },
                Allergens = { p.ProductAllergenTags.Select(pat => pat.AllergenTag.Name) },
                Countries = { p.ProductCountryTags.Select(pct => pct.CountryTag.Name) }
            });

            var response = new SearchProductsResponse
            {
                TotalCount = totalCount
            };
            response.Products.AddRange(grpcProducts);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with query: {Query}", request.Query);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<GetProductsByCategoryResponse> GetProductsByCategory(
        GetProductsByCategoryRequest request, 
        ServerCallContext context)
    {
        try
        {
            var query = _context.Products
                .Where(p => p.ProductCategoryTags.Any(pct => pct.CategoryTag.Name == request.Category));

            var totalCount = await query.CountAsync();
            
            var products = await query
                .Include(p => p.ProductAllergenTags).ThenInclude(pat => pat.AllergenTag)
                .Include(p => p.ProductCategoryTags).ThenInclude(pct => pct.CategoryTag)
                .Include(p => p.ProductIngredientTags).ThenInclude(pit => pit.IngredientTag)
                .Include(p => p.ProductCountryTags).ThenInclude(pct => pct.CountryTag)
                .Skip(request.Offset)
                .Take(request.Limit)
                .ToListAsync();

            var grpcProducts = products.Select(p => new Product
            {
                Id = p.Id.ToString(),
                Name = p.ProductName ?? "",
                Brand = p.Brands ?? "",
                Barcode = p.Code ?? "",
                ImageUrl = p.ImageUrl ?? "",
                Categories = { p.ProductCategoryTags.Select(pct => pct.CategoryTag.Name) },
                Ingredients = { p.ProductIngredientTags.Select(pit => pit.IngredientTag.Name) },
                Allergens = { p.ProductAllergenTags.Select(pat => pat.AllergenTag.Name) },
                Countries = { p.ProductCountryTags.Select(pct => pct.CountryTag.Name) }
            });

            var response = new GetProductsByCategoryResponse
            {
                TotalCount = totalCount
            };
            response.Products.AddRange(grpcProducts);

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by category: {Category}", request.Category);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<ImportProductsResponse> ImportProducts(
        ImportProductsRequest request, 
        ServerCallContext context)
    {
        try
        {
            await _productImporter.ImportAsync(request.FilePath, request.BatchSize);
            
            return new ImportProductsResponse
            {
                Success = true,
                Message = "Products imported successfully",
                ImportedCount = request.BatchSize // This should be the actual count from the importer
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing products from: {FilePath}", request.FilePath);
            return new ImportProductsResponse
            {
                Success = false,
                Message = ex.Message,
                ImportedCount = 0
            };
        }
    }
}