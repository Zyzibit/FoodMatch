using inzynierka.API.Shared.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace inzynierka.Data.Configuration;

public class UserProductsConfiguration: IEntityTypeConfiguration<UserProduct> {
    public void Configure(EntityTypeBuilder<UserProduct> builder) {
        builder.HasKey(up => new { up.UserId, up.ProductId });

        // Define relationships
        builder.HasOne(up => up.User)
            .WithMany(u => u.UserProducts)
            .HasForeignKey(up => up.UserId);

        builder.HasOne(up => up.Product)
            .WithMany(p => p.UserProducts)
            .HasForeignKey(up => up.ProductId);
    }
}