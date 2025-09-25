using inzynierka.API.Product.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace inzynierka.Data.Configuration;

public class ProductConfiguration: IEntityTypeConfiguration<Product>{
    public void Configure(EntityTypeBuilder<Product> builder) {
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => p.Code).IsUnique();
        
    }
}