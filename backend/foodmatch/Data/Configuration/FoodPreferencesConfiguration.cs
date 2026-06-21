using inzynierka.Users.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace inzynierka.Data.Configuration;

public class FoodPreferencesConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.OwnsOne(u => u.FoodPreferences, fp =>
        {
            fp.Property(p => p.IsVegan)
                .HasDefaultValue(false);
            
            fp.Property(p => p.IsVegetarian)
                .HasDefaultValue(false);
            
            fp.Property(p => p.HasGlutenIntolerance)
                .HasDefaultValue(false);
            
            fp.Property(p => p.HasLactoseIntolerance)
                .HasDefaultValue(false);
            
            fp.Property(p => p.Allergies)
                .HasConversion(
                    v => v == null || v.Count == 0 ? string.Empty : string.Join(',', v),
                    v => string.IsNullOrWhiteSpace(v) ? new List<string>() : v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                )
                .Metadata.SetValueComparer(
                    new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
                        (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()
                    )
                );
            
            fp.Property(p => p.Allergies)
                .IsRequired(false);
            
            fp.Property(p => p.Age)
                .IsRequired(false);
            
            fp.Property(p => p.Gender)
                .IsRequired(false)
                .HasConversion<string>();
            
            fp.Property(p => p.Weight)
                .IsRequired(false)
                .HasPrecision(5, 2);
            
            fp.Property(p => p.Height)
                .IsRequired(false)
                .HasPrecision(5, 2);
            
            fp.Property(p => p.ActivityLevel)
                .IsRequired(false)
                .HasConversion<string>();
            
            fp.Property(p => p.FitnessGoal)
                .IsRequired(false)
                .HasConversion<string>();
            
            fp.Property(p => p.DailyProteinGoal)
                .HasDefaultValue(0);
            
            fp.Property(p => p.DailyCarbohydrateGoal)
                .HasDefaultValue(0);
            
            fp.Property(p => p.DailyFatGoal)
                .HasDefaultValue(0);
            
            fp.Property(p => p.DailyCalorieGoal)
                .HasDefaultValue(0);
            
            fp.OwnsOne(p => p.Breakfast, breakfast =>
            {
                breakfast.Property(b => b.CaloriePercentage).HasDefaultValue(30);
                breakfast.Property(b => b.ProteinPercentage).HasDefaultValue(25);
                breakfast.Property(b => b.CarbohydratePercentage).HasDefaultValue(30);
                breakfast.Property(b => b.FatPercentage).HasDefaultValue(30);
            });
            
            fp.OwnsOne(p => p.Lunch, lunch =>
            {
                lunch.Property(l => l.CaloriePercentage).HasDefaultValue(40);
                lunch.Property(l => l.ProteinPercentage).HasDefaultValue(35);
                lunch.Property(l => l.CarbohydratePercentage).HasDefaultValue(40);
                lunch.Property(l => l.FatPercentage).HasDefaultValue(40);
            });
            
            fp.OwnsOne(p => p.Dinner, dinner =>
            {
                dinner.Property(d => d.CaloriePercentage).HasDefaultValue(25);
                dinner.Property(d => d.ProteinPercentage).HasDefaultValue(35);
                dinner.Property(d => d.CarbohydratePercentage).HasDefaultValue(25);
                dinner.Property(d => d.FatPercentage).HasDefaultValue(25);
            });
            
            fp.OwnsOne(p => p.Snack, snack =>
            {
                snack.Property(s => s.CaloriePercentage).HasDefaultValue(5);
                snack.Property(s => s.ProteinPercentage).HasDefaultValue(5);
                snack.Property(s => s.CarbohydratePercentage).HasDefaultValue(5);
                snack.Property(s => s.FatPercentage).HasDefaultValue(5);
            });
        });
    }
}

