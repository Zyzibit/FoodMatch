using foodmatch.Products.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace foodmatch.Data.Configuration;

public class ProductConfiguration: IEntityTypeConfiguration<Product>{
    public void Configure(EntityTypeBuilder<Product> builder) {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Code).IsUnique();
        
    }
}