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
        
        // Obliczanie celów kalorycznych dla poszczególnych posiłków
        int? breakfastCalories = targetCalories > 0 ? (int)(targetCalories * preferences.BreakfastCaloriePercentage / 100.0) : null;
        int? lunchCalories = targetCalories > 0 ? (int)(targetCalories * preferences.LunchCaloriePercentage / 100.0) : null;
        int? dinnerCalories = targetCalories > 0 ? (int)(targetCalories * preferences.DinnerCaloriePercentage / 100.0) : null;
        int? snackCalories = targetCalories > 0 ? (int)(targetCalories * preferences.SnackCaloriePercentage / 100.0) : null;
        
        // Obliczanie celów białka dla poszczególnych posiłków
        int? breakfastProtein = preferences.DailyProteinGoal > 0 ? (int)(preferences.DailyProteinGoal * preferences.BreakfastProteinPercentage / 100.0) : null;
        int? lunchProtein = preferences.DailyProteinGoal > 0 ? (int)(preferences.DailyProteinGoal * preferences.LunchProteinPercentage / 100.0) : null;
        int? dinnerProtein = preferences.DailyProteinGoal > 0 ? (int)(preferences.DailyProteinGoal * preferences.DinnerProteinPercentage / 100.0) : null;
        int? snackProtein = preferences.DailyProteinGoal > 0 ? (int)(preferences.DailyProteinGoal * preferences.SnackProteinPercentage / 100.0) : null;
        
        // Obliczanie celów węglowodanów dla poszczególnych posiłków
        int? breakfastCarbs = preferences.DailyCarbohydrateGoal > 0 ? (int)(preferences.DailyCarbohydrateGoal * preferences.BreakfastCarbohydratePercentage / 100.0) : null;
        int? lunchCarbs = preferences.DailyCarbohydrateGoal > 0 ? (int)(preferences.DailyCarbohydrateGoal * preferences.LunchCarbohydratePercentage / 100.0) : null;
        int? dinnerCarbs = preferences.DailyCarbohydrateGoal > 0 ? (int)(preferences.DailyCarbohydrateGoal * preferences.DinnerCarbohydratePercentage / 100.0) : null;
        int? snackCarbs = preferences.DailyCarbohydrateGoal > 0 ? (int)(preferences.DailyCarbohydrateGoal * preferences.SnackCarbohydratePercentage / 100.0) : null;
        
        // Obliczanie celów tłuszczów dla poszczególnych posiłków
        int? breakfastFat = preferences.DailyFatGoal > 0 ? (int)(preferences.DailyFatGoal * preferences.BreakfastFatPercentage / 100.0) : null;
        int? lunchFat = preferences.DailyFatGoal > 0 ? (int)(preferences.DailyFatGoal * preferences.LunchFatPercentage / 100.0) : null;
        int? dinnerFat = preferences.DailyFatGoal > 0 ? (int)(preferences.DailyFatGoal * preferences.DinnerFatPercentage / 100.0) : null;
        int? snackFat = preferences.DailyFatGoal > 0 ? (int)(preferences.DailyFatGoal * preferences.SnackFatPercentage / 100.0) : null;
        
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
            BreakfastProteinPercentage = preferences.BreakfastProteinPercentage,
            LunchProteinPercentage = preferences.LunchProteinPercentage,
            DinnerProteinPercentage = preferences.DinnerProteinPercentage,
            SnackProteinPercentage = preferences.SnackProteinPercentage,
            BreakfastCarbohydratePercentage = preferences.BreakfastCarbohydratePercentage,
            LunchCarbohydratePercentage = preferences.LunchCarbohydratePercentage,
            DinnerCarbohydratePercentage = preferences.DinnerCarbohydratePercentage,
            SnackCarbohydratePercentage = preferences.SnackCarbohydratePercentage,
            BreakfastFatPercentage = preferences.BreakfastFatPercentage,
            LunchFatPercentage = preferences.LunchFatPercentage,
            DinnerFatPercentage = preferences.DinnerFatPercentage,
            SnackFatPercentage = preferences.SnackFatPercentage,
            CalculatedBMR = bmr,
            CalculatedDailyCalories = dailyCalories,
            BreakfastCalories = breakfastCalories,
            LunchCalories = lunchCalories,
            DinnerCalories = dinnerCalories,
            SnackCalories = snackCalories,
            BreakfastProteinGoal = breakfastProtein,
            LunchProteinGoal = lunchProtein,
            DinnerProteinGoal = dinnerProtein,
            SnackProteinGoal = snackProtein,
            BreakfastCarbohydrateGoal = breakfastCarbs,
            LunchCarbohydrateGoal = lunchCarbs,
            DinnerCarbohydrateGoal = dinnerCarbs,
            SnackCarbohydrateGoal = snackCarbs,
            BreakfastFatGoal = breakfastFat,
            LunchFatGoal = lunchFat,
            DinnerFatGoal = dinnerFat,
            SnackFatGoal = snackFat
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

