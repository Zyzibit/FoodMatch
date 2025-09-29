
using inzynierka.Products.Model.Tag.AllergenTag;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace inzynierka.Data.Configuration;

public class ProductAllergenTagConfiguration: IEntityTypeConfiguration<ProductAllergenTag> {
    public void Configure(EntityTypeBuilder<ProductAllergenTag> builder) {
        builder.HasKey(p => new { p.ProductId, p.AllergenTagId });
        builder.HasOne(p=>p.Product).WithMany(p=>p.ProductAllergenTags).HasForeignKey(p=>p.ProductId);
        builder.HasOne(p=>p.AllergenTag).WithMany(t=>t.ProductAllergenTags).HasForeignKey(p=>p.AllergenTagId);
        
    }
}