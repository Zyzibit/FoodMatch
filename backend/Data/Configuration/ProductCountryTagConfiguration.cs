using inzynierka.Products.Model.Tag.CountryTag;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace inzynierka.Data.Configuration;

public class ProductCountryTagConfiguration: IEntityTypeConfiguration<ProductCountryTag> {
    public void Configure(EntityTypeBuilder<ProductCountryTag> builder) {
        builder.HasKey(p=>new { p.ProductId, p.CountryTagId });
        builder.HasOne(p=>p.Product).WithMany(p=>p.ProductCountryTags).HasForeignKey(p=>p.ProductId);
        builder.HasOne(p=>p.CountryTag).WithMany(t=>t.ProductCountryTags).HasForeignKey(p=>p.CountryTagId);
    }
}