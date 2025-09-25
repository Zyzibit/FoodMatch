using inzynierka.API.Product.Model.Tag.CategoryTag;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace inzynierka.Data.Configuration;

public class ProductCategoryTagConfiguration: IEntityTypeConfiguration<ProductCategoryTag> {
    public void Configure(EntityTypeBuilder<ProductCategoryTag> builder) {
        builder.HasKey(p=> new { p.ProductId, p.CategoryTagId });
        builder.HasOne(p=>p.Product).WithMany(p=>p.ProductCategoryTags).HasForeignKey(p=>p.ProductId);
        builder.HasOne(p=>p.CategoryTag).WithMany(t=>t.ProductCategoryTags).HasForeignKey(p=>p.CategoryTagId);
    }
}