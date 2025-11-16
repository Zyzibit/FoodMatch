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
            
            fp.Property(p => p.DailyProteinGoal)
                .HasDefaultValue(0);
            
            fp.Property(p => p.DailyCarbohydrateGoal)
                .HasDefaultValue(0);
            
            fp.Property(p => p.DailyFatGoal)
                .HasDefaultValue(0);
            
            fp.Property(p => p.DailyCalorieGoal)
                .HasDefaultValue(0);
            
            fp.Property(p => p.BreakfastCaloriePercentage)
                .HasDefaultValue(30);
            
            fp.Property(p => p.LunchCaloriePercentage)
                .HasDefaultValue(40);
            
            fp.Property(p => p.DinnerCaloriePercentage)
                .HasDefaultValue(25);
            
            fp.Property(p => p.SnackCaloriePercentage)
                .HasDefaultValue(5);
            

            fp.Property(p => p.BreakfastProteinPercentage)
                .HasDefaultValue(25);
            
            fp.Property(p => p.LunchProteinPercentage)
                .HasDefaultValue(35);
            
            fp.Property(p => p.DinnerProteinPercentage)
                .HasDefaultValue(35);
            
            fp.Property(p => p.SnackProteinPercentage)
                .HasDefaultValue(5);
            

            fp.Property(p => p.BreakfastCarbohydratePercentage)
                .HasDefaultValue(30);
            
            fp.Property(p => p.LunchCarbohydratePercentage)
                .HasDefaultValue(40);
            
            fp.Property(p => p.DinnerCarbohydratePercentage)
                .HasDefaultValue(25);
            
            fp.Property(p => p.SnackCarbohydratePercentage)
                .HasDefaultValue(5);

            
            fp.Property(p => p.BreakfastFatPercentage)
                .HasDefaultValue(30);
            
            fp.Property(p => p.LunchFatPercentage)
                .HasDefaultValue(40);
            
            fp.Property(p => p.DinnerFatPercentage)
                .HasDefaultValue(25);
            
            fp.Property(p => p.SnackFatPercentage)
                .HasDefaultValue(5);
        });
    }
}

