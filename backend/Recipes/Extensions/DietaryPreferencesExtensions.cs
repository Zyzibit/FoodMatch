using inzynierka.Recipes.Model;
using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.Users.Extensions;
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
    
    public static DietaryPreferences MergeWithUserPreferences(
        this DietaryPreferences? requestPreferences,
        FoodPreferencesDto? userPreferences)
    {
        var userDietaryPrefs = userPreferences.ToDietaryPreferences();
        
        if (requestPreferences == null)
        {
            return userDietaryPrefs ?? new DietaryPreferences();
        }

        if (userDietaryPrefs == null)
        {
            return requestPreferences;
        }

        
        var merged = new DietaryPreferences
        {
            IsVegetarian = requestPreferences.IsVegetarian || userDietaryPrefs.IsVegetarian,
            IsVegan = requestPreferences.IsVegan || userDietaryPrefs.IsVegan,
            IsGlutenFree = requestPreferences.IsGlutenFree || userDietaryPrefs.IsGlutenFree,
            IsLactoseFree = requestPreferences.IsLactoseFree || userDietaryPrefs.IsLactoseFree,
            Allergies = requestPreferences.Allergies.Any() 
                ? requestPreferences.Allergies 
                : userDietaryPrefs.Allergies,
            DislikedIngredients = requestPreferences.DislikedIngredients.Any() 
                ? requestPreferences.DislikedIngredients 
                : userDietaryPrefs.DislikedIngredients,
            FitnessGoal = requestPreferences.FitnessGoal ?? userDietaryPrefs.FitnessGoal,
            DailyCalorieGoal = requestPreferences.DailyCalorieGoal ?? userDietaryPrefs.DailyCalorieGoal,
            DailyProteinGoal = requestPreferences.DailyProteinGoal ?? userDietaryPrefs.DailyProteinGoal,
            DailyCarbohydrateGoal = requestPreferences.DailyCarbohydrateGoal ?? userDietaryPrefs.DailyCarbohydrateGoal,
            DailyFatGoal = requestPreferences.DailyFatGoal ?? userDietaryPrefs.DailyFatGoal,
            TargetMealCalories = requestPreferences.TargetMealCalories ?? userDietaryPrefs.TargetMealCalories,
            TargetMealProtein = requestPreferences.TargetMealProtein ?? userDietaryPrefs.TargetMealProtein,
            TargetMealCarbohydrates = requestPreferences.TargetMealCarbohydrates ?? userDietaryPrefs.TargetMealCarbohydrates,
            TargetMealFat = requestPreferences.TargetMealFat ?? userDietaryPrefs.TargetMealFat
        };
        return merged;
    }
}


