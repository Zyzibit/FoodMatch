using inzynierka.Users.Responses;

namespace inzynierka.Recipes.Model;

public enum MealType
{
    Breakfast,
    Lunch,
    Dinner,
    Snack
}

public record MealNutritionalGoals(
    int? Calories,
    int? Protein,
    int? Carbohydrates,
    int? Fat);

public static class MealTypeExtensions
{
    private static readonly Dictionary<MealType, Func<FoodPreferencesDto, MealNutritionalGoals>> NutritionalGoalsMapper = new()
    {
        [MealType.Breakfast] = prefs => new MealNutritionalGoals(
            prefs.BreakfastCalories,
            prefs.BreakfastProteinGoal,
            prefs.BreakfastCarbohydrateGoal,
            prefs.BreakfastFatGoal),
        
        [MealType.Lunch] = prefs => new MealNutritionalGoals(
            prefs.LunchCalories,
            prefs.LunchProteinGoal,
            prefs.LunchCarbohydrateGoal,
            prefs.LunchFatGoal),
        
        [MealType.Dinner] = prefs => new MealNutritionalGoals(
            prefs.DinnerCalories,
            prefs.DinnerProteinGoal,
            prefs.DinnerCarbohydrateGoal,
            prefs.DinnerFatGoal),
        
        [MealType.Snack] = prefs => new MealNutritionalGoals(
            prefs.SnackCalories,
            prefs.SnackProteinGoal,
            prefs.SnackCarbohydrateGoal,
            prefs.SnackFatGoal)
    };
    
    public static MealNutritionalGoals GetNutritionalGoals(this MealType mealType, FoodPreferencesDto userPreferences)
    {
        if (!NutritionalGoalsMapper.TryGetValue(mealType, out var mapper))
        {
            throw new ArgumentException($"Unknown meal type: {mealType}", nameof(mealType));
        }
        
        return mapper(userPreferences);
    }
    
    public static string GetName(this MealType mealType) => mealType.ToString();
    
    public static MealType? Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;
            
        return Enum.TryParse<MealType>(value, ignoreCase: true, out var result) ? result : null;
    }
    
    public static bool IsValid(string? value) => Parse(value).HasValue;
    
    public static IEnumerable<MealType> GetAll() => Enum.GetValues<MealType>();
    
    public static IEnumerable<string> GetAllNames() => GetAll().Select(m => m.GetName());
}

