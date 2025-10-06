namespace inzynierka.Products.Contracts.Models;

public class ProductResult
{
    public bool Success { get; set; }
    public ProductInfo? Product { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ProductInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public string Barcode { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public List<string> Categories { get; set; } = new();
    public List<string> Ingredients { get; set; } = new();
    public List<string> Allergens { get; set; } = new();
    public List<string> Countries { get; set; } = new();
    public NutritionInfo? Nutrition { get; set; }
    public string? NutritionGrade { get; set; }
    public string? EcoScoreGrade { get; set; }
}

public class ProductSearchResult
{
    public bool Success { get; set; }
    public List<ProductInfo> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ProductSearchQuery
{
    public string? Query { get; set; }
    public List<string>? Categories { get; set; }
    public List<string>? Allergens { get; set; }
    public List<string>? Ingredients { get; set; }
    public string? Brand { get; set; }
    public int Limit { get; set; } = 10;
    public int Offset { get; set; } = 0;
}

public class ProductCategoryResult
{
    public bool Success { get; set; }
    public List<ProductInfo> Products { get; set; } = new();
    public int TotalCount { get; set; }
    public string? ErrorMessage { get; set; }
}

public class ProductImportResult
{
    public bool Success { get; set; }
    public int ImportedCount { get; set; }
    public int FailedCount { get; set; }
    public string? ErrorMessage { get; set; }
    public List<string>? Warnings { get; set; }
}

public class ProductCategory
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int ProductCount { get; set; }
}

public class NutritionInfo
{
    public double? Energy { get; set; }
    public double? Fat { get; set; }
    public double? SaturatedFat { get; set; }
    public double? Carbohydrates { get; set; }
    public double? Sugars { get; set; }
    public double? Fiber { get; set; }
    public double? Proteins { get; set; }
    public double? Salt { get; set; }
    public double? Sodium { get; set; }
}

public class ProductNutritionResult
{
    public bool Success { get; set; }
    public NutritionInfo? Nutrition { get; set; }
    public string? ErrorMessage { get; set; }
}