 Repository Layer Implementation for OpenFoodFacts Module

## Overview
This implementation adds a repository layer to the OpenFoodFacts module, separating data access concerns from business logic and improving maintainability of the import process.

## Architecture

### 1. Repository Interface (`IOpenFoodFactsRepository`)
Located in: `Products/OpenFoodFacts/Repositories/IOpenFoodFactsRepository.cs`

Provides specialized data access operations for OpenFoodFacts import:

#### Tag Management Operations
- **Generic Tag Operations**: CRUD operations for all tag types
- **Tag Dictionary**: Fast lookups for existing tags
- **Specialized Tag Methods**: Type-specific operations for Country, Category, Allergen, and Ingredient tags

#### Product Import Operations  
- **Bulk Operations**: High-performance bulk insert for products
- **Duplicate Detection**: Efficient checking for existing product codes
- **Code Management**: Loading and managing product code collections

#### Cache Operations
- **Tag Caching**: Redis-based caching for tag lookups
- **Cache Management**: Loading and saving tag caches for performance

### 2. Repository Implementation (`OpenFoodFactsRepository`)
Located in: `Products/OpenFoodFacts/Repositories/OpenFoodFactsRepository.cs`

Key features:
- **High Performance**: Optimized for bulk import operations
- **Redis Integration**: Intelligent caching with Redis fallback
- **Error Handling**: Comprehensive error handling with structured logging
- **Generic Constraints**: Type-safe operations with `ITagEntity` constraints

### 3. Service Registration Extension (`OpenFoodFactsServiceExtensions`)
Located in: `Products/OpenFoodFacts/Extensions/OpenFoodFactsServiceExtensions.cs`

Provides modular service registration:
```csharp
builder.Services.AddOpenFoodFactsServices();
```

## Benefits

### 1. Separation of Concerns
- **ProductImporter**: Focuses on orchestration and business logic
- **OpenFoodFactsRepository**: Handles all data access operations  
- **Caching Layer**: Abstracted behind repository interface

### 2. Performance Optimizations
- **Bulk Operations**: Efficient EFCore.BulkExtensions usage
- **Smart Caching**: Redis-first with database fallback
- **Memory Management**: Optimized collections and async streaming

### 3. Testability
- **Repository Abstraction**: Easy mocking for unit tests
- **Isolated Dependencies**: Clear boundaries between layers
- **Dependency Injection**: Full IoC container support

### 4. Maintainability
- **Centralized Data Access**: Single point for import-related queries
- **Consistent Error Handling**: Structured logging throughout
- **Type Safety**: Generic constraints prevent runtime errors

## Usage Examples

### Repository Pattern Usage
```csharp
// Constructor injection in ProductImporter
public ProductImporter(
    IOpenFoodFactsDeserializer deserializer,
    IMapper mapper,
    IOpenFoodFactsRepository repository,
    ILogger<ProductImporter> logger)
{
    _repository = repository;
    // ...
}

// Using repository methods
var existingCodes = await _repository.GetExistingProductCodesAsync();
var countries = await _repository.GetOrCreateCountryTagsAsync(countryNames);
await _repository.BulkInsertProductsAsync(products);
```

### Cache Operations
```csharp
// Load cache with Redis fallback
var tagCache = await _repository.LoadTagCacheAsync<CategoryTag>();

// Save updated cache
await _repository.SaveTagCacheAsync<CategoryTag>(updatedCache);
```

### Tag Management
```csharp
// Get or create tags automatically
var newTags = await _repository.CreateTagsAsync<IngredientTag>(ingredientNames);

// Type-safe tag operations
var allergens = await _repository.GetOrCreateAllergenTagsAsync(allergenNames);
```

## Migration from Direct DbContext Access

### Before (ProductImporter)
```csharp
public ProductImporter(
    IServiceScopeFactory scopeFactory,
    // ... other dependencies
)

private async Task LoadExistingProductCodesAsync(AppDbContext db)
{
    await foreach (var code in db.Products
        .AsNoTracking()
        .Select(p => p.Code)
        .AsAsyncEnumerable())
    {
        // Direct database access
    }
}
```

### After (ProductImporter) 
```csharp
public ProductImporter(
    IOpenFoodFactsRepository repository,
    // ... other dependencies  
)

private async Task LoadExistingProductCodesAsync()
{
    var existingCodes = await _repository.GetExistingProductCodesAsync();
    // Repository abstraction
}
```

## Configuration

### Service Registration
The repository is registered through the extension method:

```csharp
// In ProductsServiceExtensions
services.AddOpenFoodFactsServices();
```

Which registers:
- `IOpenFoodFactsRepository` ? `OpenFoodFactsRepository`
- `IProductImporter` ? `ProductImporter`
- `IOpenFoodFactsDeserializer` ? `OpenFoodFactsDeserializer`

### Cache Configuration
```csharp
private const int BULK_BATCH_SIZE = 10_000;
private static readonly TimeSpan CACHE_EXPIRY = TimeSpan.FromHours(24);
```

## Performance Characteristics

### Bulk Operations
- **Batch Size**: 10,000 products per batch
- **Memory Efficient**: Streaming enumeration for large datasets
- **Transaction Scope**: Optimized bulk insert with EFCore.BulkExtensions

### Caching Strategy
- **Redis First**: Primary cache storage
- **Database Fallback**: Automatic fallback if Redis unavailable  
- **24-hour TTL**: Configurable cache expiration
- **Smart Updates**: Cache updates during import process

### Memory Management
- **HashSet<string>**: Efficient duplicate detection for product codes
- **ConcurrentDictionary**: Thread-safe tag caches
- **Async Enumeration**: Memory-efficient streaming of large collections

## Error Handling

### Repository Level
```csharp
try
{
    return await _context.Set<T>().AsNoTracking().ToListAsync();
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error getting all tags of type {TagType}", typeof(T).Name);
    throw;
}
```

### Service Level
```csharp
try
{
    await _repository.BulkInsertProductsAsync(batch);
}
catch (Exception ex)
{
    _logger.LogError(ex, "Failed to insert batch of {Count} products", batch.Count);
    throw;
}
```

## Future Enhancements

1. **Partitioning**: Implement table partitioning for very large datasets
2. **Parallel Processing**: Add parallel tag creation for better performance  
3. **Metrics**: Add performance metrics and monitoring
4. **Retry Logic**: Implement retry policies for transient failures
5. **Compression**: Add compression for cached tag data
6. **Validation**: Enhanced data validation at repository level

## Notes

- **Tag Cache Item**: Shared record type for efficient tag caching
- **Generic Constraints**: `ITagEntity` ensures type safety across operations
- **Logging**: Comprehensive structured logging for debugging and monitoring
- **Redis Integration**: Seamless integration with existing Redis infrastructure
- **Backward Compatibility**: Maintains all existing import functionality