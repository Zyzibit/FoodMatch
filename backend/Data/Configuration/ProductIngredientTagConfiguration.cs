using inzynierka.API.Product.Model.Tag.IngredientTag;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace inzynierka.Data.Configuration;

public class ProductIngredientTagConfiguration : IEntityTypeConfiguration<ProductIngredientTag>
{
    public void Configure(EntityTypeBuilder<ProductIngredientTag> builder) {
        builder.HasKey(p => new { p.ProductId, p.IngredientTagId });
        builder.HasOne(p=>p.Product).WithMany(p=>p.ProductIngredientTags).HasForeignKey(p=>p.ProductId);
        
        builder.HasOne(p=>p.IngredientTag).WithMany(t=>t.ProductIngredientTags).HasForeignKey(p=>p.IngredientTagId);
    }
}