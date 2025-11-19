using inzynierka.Recipes.Model;
using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.Users.Responses;

namespace inzynierka.Recipes.Extensions;

public static class DietaryPreferencesExtensions
{
    public static bool ApplyMealTypeGoals(
        this DietaryPreferences preferences, 
        string mealType, 
        FoodPreferencesDto userPreferences)
    {
        var parsedMealType = MealTypeExtensions.Parse(mealType);
        
        if (!parsedMealType.HasValue)
        {
            return false;
        }

        preferences.MealType = mealType;

        var mealGoals = parsedMealType.Value.GetNutritionalGoals(userPreferences);
        
        preferences.TargetMealCalories ??= mealGoals.Calories;
        preferences.TargetMealProtein ??= mealGoals.Protein;
        preferences.TargetMealCarbohydrates ??= mealGoals.Carbohydrates;
        preferences.TargetMealFat ??= mealGoals.Fat;

        preferences.DailyCalorieGoal ??= userPreferences.DailyCalorieGoal ?? userPreferences.CalculatedDailyCalories;
        preferences.DailyProteinGoal ??= userPreferences.DailyProteinGoal;
        preferences.DailyCarbohydrateGoal ??= userPreferences.DailyCarbohydrateGoal;
        preferences.DailyFatGoal ??= userPreferences.DailyFatGoal;

        return true;
    }
}


