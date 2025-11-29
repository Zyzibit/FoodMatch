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
            prefs.Breakfast?.CaloriesGoal,
            prefs.Breakfast?.ProteinGoal,
            prefs.Breakfast?.CarbohydrateGoal,
            prefs.Breakfast?.FatGoal),
        
        [MealType.Lunch] = prefs => new MealNutritionalGoals(
            prefs.Lunch?.CaloriesGoal,
            prefs.Lunch?.ProteinGoal,
            prefs.Lunch?.CarbohydrateGoal,
            prefs.Lunch?.FatGoal),
        
        [MealType.Dinner] = prefs => new MealNutritionalGoals(
            prefs.Dinner?.CaloriesGoal,
            prefs.Dinner?.ProteinGoal,
            prefs.Dinner?.CarbohydrateGoal,
            prefs.Dinner?.FatGoal),
        
        [MealType.Snack] = prefs => new MealNutritionalGoals(
            prefs.Snack?.CaloriesGoal,
            prefs.Snack?.ProteinGoal,
            prefs.Snack?.CarbohydrateGoal,
            prefs.Snack?.FatGoal)
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

