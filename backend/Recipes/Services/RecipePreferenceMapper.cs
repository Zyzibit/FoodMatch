using inzynierka.Recipes.Model;
using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.Users.Responses;

namespace inzynierka.Recipes.Services;


public class RecipePreferenceMapper : IRecipePreferenceMapper
{
    private readonly ILogger<RecipePreferenceMapper> _logger;

    public RecipePreferenceMapper(ILogger<RecipePreferenceMapper> logger)
    {
        _logger = logger;
    }

    public DietaryPreferences? MapFromUserPreferences(FoodPreferencesDto? userPreferences)
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
            DailyCalorieGoal = userPreferences.DailyCalorieGoal,
            DailyProteinGoal = userPreferences.DailyProteinGoal,
            DailyCarbohydrateGoal = userPreferences.DailyCarbohydrateGoal,
            DailyFatGoal = userPreferences.DailyFatGoal
        };
    }

    public void ApplyMealTypeGoals(DietaryPreferences preferences, string mealType, FoodPreferencesDto userPreferences)
    {
        var parsedMealType = MealTypeExtensions.Parse(mealType);
        
        if (!parsedMealType.HasValue)
        {
            _logger.LogWarning("Invalid meal type: {MealType}", mealType);
            return;
        }

        preferences.MealType = mealType;

        var mealGoals = parsedMealType.Value.GetNutritionalGoals(userPreferences);
        
        preferences.TargetMealCalories ??= mealGoals.Calories;
        preferences.TargetMealProtein ??= mealGoals.Protein;
        preferences.TargetMealCarbohydrates ??= mealGoals.Carbohydrates;
        preferences.TargetMealFat ??= mealGoals.Fat;

        _logger.LogInformation(
            "Calculated nutritional goals for {MealType}: {Calories} kcal, {Proteins}g protein, {Carbs}g carbs, {Fat}g fat",
            mealType, mealGoals.Calories, mealGoals.Protein, mealGoals.Carbohydrates, mealGoals.Fat);

        preferences.DailyCalorieGoal ??= userPreferences.DailyCalorieGoal ?? userPreferences.CalculatedDailyCalories;
        preferences.DailyProteinGoal ??= userPreferences.DailyProteinGoal;
        preferences.DailyCarbohydrateGoal ??= userPreferences.DailyCarbohydrateGoal;
        preferences.DailyFatGoal ??= userPreferences.DailyFatGoal;
    }
}

