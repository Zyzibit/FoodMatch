using inzynierka.Users.Model;
using inzynierka.Data.Configuration;
using inzynierka.Products.Model;
using inzynierka.Products.Model.Tag.AllergenTag;
using inzynierka.Products.Model.Tag.CategoryTag;
using inzynierka.Products.Model.Tag.CountryTag;
using inzynierka.Products.Model.Tag.IngredientTag;
using inzynierka.Auth.Model;
using inzynierka.MealPlans.Model;
using inzynierka.Receipts.Model;
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
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    
    public DbSet<Receipt> Receipts { get; set; }
    public DbSet<ReceiptIngredient> ReceiptIngredients { get; set; }
    public DbSet<Unit> Units { get; set; }
    
    public DbSet<MealPlan> MealPlans { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
        modelBuilder.ApplyConfiguration(new ProductAllergenTagConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCategoryTagConfiguration());
        modelBuilder.ApplyConfiguration(new ProductCountryTagConfiguration());
        modelBuilder.ApplyConfiguration(new ProductIngredientTagConfiguration());
        modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
        modelBuilder.ApplyConfiguration(new FoodPreferencesConfiguration());
        
        modelBuilder.Entity<Receipt>()
            .HasKey(r => r.Id);

        modelBuilder.Entity<ReceiptIngredient>()
            .HasKey(ri => new { ri.ReceiptId, ri.ProductId }); 

        modelBuilder.Entity<ReceiptIngredient>()
            .HasOne(ri => ri.Receipt)
            .WithMany(r => r.Ingredients)
            .HasForeignKey(ri => ri.ReceiptId);


        modelBuilder.Entity<ReceiptIngredient>()
            .HasOne(ri => ri.Product)
            .WithMany(p => p.ReceiptIngredients)
            .HasForeignKey(ri => ri.ProductId)
            .HasPrincipalKey(p => p.Id); 

        modelBuilder.Entity<ReceiptIngredient>()
            .HasOne(ri => ri.Unit)
            .WithMany()
            .HasForeignKey(ri => ri.UnitId);
            
        modelBuilder.Entity<Unit>()
            .HasKey(u => u.UnitId);
        modelBuilder.Entity<Receipt>()
            .HasOne(r => r.User)            
            .WithMany(u => u.Receipts)      
            .HasForeignKey(r => r.UserId)    
            .IsRequired()                   
            .OnDelete(DeleteBehavior.Cascade); 
        
        
        modelBuilder.Entity<MealPlan>()
            .HasOne(mp => mp.User)
            .WithMany(u => u.MealPlans)
            .HasForeignKey(mp => mp.UserId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MealPlan>()
            .HasOne(mp => mp.Receipt)
            .WithMany()
            .HasForeignKey(mp => mp.ReceiptId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}