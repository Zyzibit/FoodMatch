 using inzynierka.Users.Model;
using inzynierka.Users.Responses;

namespace inzynierka.Users.Extensions;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public static FoodPreferencesDto ToDto(this FoodPreferences preferences)
    {
        int? bmr = null;
        int? dailyCalories = null;
        
        if (preferences.Age.HasValue && preferences.Gender.HasValue && 
            preferences.Weight.HasValue && preferences.Height.HasValue)
        {
            bmr = CalculateBMR(preferences.Age.Value, preferences.Gender.Value, 
                             preferences.Weight.Value, preferences.Height.Value);
            
            if (preferences.ActivityLevel.HasValue && bmr.HasValue)
            {
                dailyCalories = (int)(bmr.Value * GetPALMultiplier(preferences.ActivityLevel.Value));
            }
        }
        
        var targetCalories = preferences.DailyCalorieGoal > 0 
            ? preferences.DailyCalorieGoal 
            : (dailyCalories ?? 0);
        
        return new FoodPreferencesDto
        {
            IsVegan = preferences.IsVegan,
            IsVegetarian = preferences.IsVegetarian,
            HasGlutenIntolerance = preferences.HasGlutenIntolerance,
            HasLactoseIntolerance = preferences.HasLactoseIntolerance,
            Allergies = preferences.Allergies,
            Age = preferences.Age,
            Gender = preferences.Gender?.ToString(),
            Weight = preferences.Weight,
            Height = preferences.Height,
            ActivityLevel = preferences.ActivityLevel?.ToString(),
            DailyCarbohydrateGoal = preferences.DailyCarbohydrateGoal,
            DailyProteinGoal = preferences.DailyProteinGoal,
            DailyFatGoal = preferences.DailyFatGoal,
            DailyCalorieGoal = preferences.DailyCalorieGoal,
            BreakfastCaloriePercentage = preferences.BreakfastCaloriePercentage,
            LunchCaloriePercentage = preferences.LunchCaloriePercentage,
            DinnerCaloriePercentage = preferences.DinnerCaloriePercentage,
            SnackCaloriePercentage = preferences.SnackCaloriePercentage,
            CalculatedBMR = bmr,
            CalculatedDailyCalories = dailyCalories,
            BreakfastCalories = targetCalories > 0 ? (int)(targetCalories * preferences.BreakfastCaloriePercentage / 100.0) : null,
            LunchCalories = targetCalories > 0 ? (int)(targetCalories * preferences.LunchCaloriePercentage / 100.0) : null,
            DinnerCalories = targetCalories > 0 ? (int)(targetCalories * preferences.DinnerCaloriePercentage / 100.0) : null,
            SnackCalories = targetCalories > 0 ? (int)(targetCalories * preferences.SnackCaloriePercentage / 100.0) : null
        };
    }
    
    private static int CalculateBMR(int age, Gender gender, decimal weight, decimal height)
    {
        decimal bmr = (10 * weight) + (6.25m * height) - (5 * age);
        
        if (gender == Gender.Male)
        {
            bmr += 5;
        }
        else if (gender == Gender.Female)
        {
            bmr -= 161;
        }
        
        return (int)Math.Round(bmr);
    }
    
    private static decimal GetPALMultiplier(PhysicalActivityLevel level)
    {
        return level switch
        {
            PhysicalActivityLevel.Sedentary => 1.2m,
            PhysicalActivityLevel.LightlyActive => 1.375m,
            PhysicalActivityLevel.ModeratelyActive => 1.55m,
            PhysicalActivityLevel.VeryActive => 1.725m,
            PhysicalActivityLevel.ExtraActive => 1.9m,
            _ => 1.2m
        };
    }
}

