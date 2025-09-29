using AutoMapper;
using inzynierka.Data;
using inzynierka.OpenFoodFacts.JsonlReader.Services;
using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.CountryTag;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.OpenFoodFacts.Import;

public class ProductImporter : IProductImporter
{
    private readonly IOpenFoodFactsDeserializer _deserializer;
    private readonly IMapper _mapper;
    private readonly IServiceScopeFactory _scopeFactory;

    public ProductImporter(
        IOpenFoodFactsDeserializer deserializer,
        IMapper mapper, IServiceScopeFactory scopeFactory)
    {
        _deserializer = deserializer;
        _mapper = mapper;
        _scopeFactory = scopeFactory;
    }

    //maxProducts can be removed for final
public async Task ImportAsync(string path, int maxProducts)
{

    int count = 0;    //for testing purposes
    var db = _scopeFactory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();

    await foreach (var jsonProduct in _deserializer.DeserializeFromJsonlFileAsync(path))
    {
        if (jsonProduct.CountriesTags?.Contains("en:poland") == true)
        {
            var product = _mapper.Map<Product>(jsonProduct);
            
            var countryTags = await GetOrCreateTagsAsync(db, db.CountryTags, jsonProduct.CountriesTags);
            var categoryTags = await GetOrCreateTagsAsync(db, db.CategoryTags, jsonProduct.CategoriesTags);
            var allergenTags = await GetOrCreateTagsAsync(db, db.AllergenTags, jsonProduct.AllergensTags);
            var ingredientTags= await GetOrCreateTagsAsync(db, db.IngredientTags, jsonProduct.IngredientsTags);
            
            
            
            //many to many relationships from list <string> to object with id, for normalziing a database
            product.ProductIngredientTags = ingredientTags
                .Select(tag => new ProductIngredientTag { IngredientTagId = tag.Id }).ToList();

            product.ProductCountryTags = countryTags
                .Select(tag => new ProductCountryTag { CountryTagId = tag.Id }).ToList();

            product.ProductCategoryTags = categoryTags
                .Select(tag => new ProductCategoryTag { CategoryTagId = tag.Id }).ToList();

            product.ProductAllergenTags = allergenTags
                .Select(tag => new ProductAllergenTag { AllergenTagId = tag.Id }).ToList();

            
            await db.Products.AddAsync(product);
            await db.SaveChangesAsync();

            
          
            Console.WriteLine($"[{count}] Zaimportowano: {product.ProductName}");
            count++;  //can be removed for final

            if (count >= maxProducts)  //can be removed for final
                break;  //can be removed for final
        }
    }
    
}

// Returns a list of tags from the database, creating any missing ones.
    private async Task<List<TTag>> GetOrCreateTagsAsync<TTag>(
        DbContext db,
        DbSet<TTag> dbSet,
        List<string> names
    )
        where TTag : class, ITagEntity, new()
    {
        names = names?.Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList() ?? new List<string>();

        if (!names.Any())
            return new List<TTag>();
        
        //existing tags in database
        var existing = await dbSet.Where(t => names.Contains(t.Name)).ToListAsync();
        //missing tags in database
        var missing = names.Except(existing.Select(t => t.Name)).ToList();

        //create new object for all missing tags
        var newTags = missing.Select(name => new TTag { Name = name }).ToList();
        dbSet.AddRange(newTags);
        await db.SaveChangesAsync();

        return await dbSet.Where(t => names.Contains(t.Name)).ToListAsync();
    }


}
