using System.Data.Common;
using inzynierka.API.Product.Model;
using inzynierka.API.Product.Model.Tag.AllergenTag;
using inzynierka.API.Product.Model.Tag.CategoryTag;
using inzynierka.API.Product.Model.Tag.CountryTag;
using inzynierka.API.Product.Model.Tag.IngredientTag;
using inzynierka.API.User.Model;
using inzynierka.Data.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Data;

public class AppDbContext : IdentityDbContext<User> {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<CategoryTag> CategoryTags { get; set; }
    public DbSet<AllergenTag> AllergenTags { get; set; }
    public DbSet<IngredientTag> IngredientTags  { get; set; }
    public DbSet<CountryTag> CountryTags { get; set; }
    public DbSet<TokenInfo> TokenInfos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductAllergenTagConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCategoryTagConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCountryTagConfiguration());
        modelBuilder.ApplyConfiguration(new ProductIngredientTagConfiguration());
        modelBuilder.ApplyConfiguration(new UserProductsConfiguration());
    }
    

}