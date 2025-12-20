using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.UserPreferences.Responses;

namespace inzynierka.Users.Extensions;

public static class FoodPreferencesExtensions
{
    public static DietaryPreferences? ToDietaryPreferences(this FoodPreferencesDto? userPreferences)
    {
        if (userPreferences == null)
        {
            return null;
        }

        return new DietaryPreferences
        {
            IsVegan = userPreferences.IsVegan ?? false,
            IsVegetarian = userPreferences.IsVegetarian ?? false,
            IsGlutenFree = userPreferences.HasGlutenIntolerance ?? false,
            IsLactoseFree = userPreferences.HasLactoseIntolerance ?? false,
            Allergies = userPreferences.Allergies ?? new List<string>(),
            DislikedIngredients = new List<string>(),
            FitnessGoal = userPreferences.FitnessGoal,
            DailyCalorieGoal = userPreferences.DailyCalorieGoal,
            DailyProteinGoal = userPreferences.DailyProteinGoal,
            DailyCarbohydrateGoal = userPreferences.DailyCarbohydrateGoal,
            DailyFatGoal = userPreferences.DailyFatGoal
        };
    }
}

