using inzynierka.Auth.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace inzynierka.Data.Configuration;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);
        
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(500);
            
        builder.Property(rt => rt.UserId)
            .IsRequired();
            
        builder.Property(rt => rt.ExpiryDate)
            .IsRequired();
            
        builder.Property(rt => rt.CreatedAt)
            .IsRequired();
            
        builder.Property(rt => rt.DeviceId)
            .HasMaxLength(100);
            
        builder.Property(rt => rt.UserAgent)
            .HasMaxLength(500);
            
        builder.Property(rt => rt.IpAddress)
            .HasMaxLength(45); // IPv6 max length
        
        builder.HasOne(rt => rt.User)
            .WithMany()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasIndex(rt => rt.Token)
            .IsUnique();
            
        builder.HasIndex(rt => new { rt.UserId, rt.DeviceId });
        
        builder.HasIndex(rt => rt.ExpiryDate);
    }
}