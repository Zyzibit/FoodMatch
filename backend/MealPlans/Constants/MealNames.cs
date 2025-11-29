using inzynierka.Recipes.Model;

namespace inzynierka.MealPlans.Constants;

public static class MealNames
{
    public static string Breakfast => MealType.Breakfast.GetName();
    public static string Lunch => MealType.Lunch.GetName();
    public static string Dinner => MealType.Dinner.GetName();
    public static string Snack => MealType.Snack.GetName();
    
    public static readonly HashSet<string> AllowedMealNames = 
        MealTypeExtensions.GetAllNames().ToHashSet();
    
    public static bool IsValidMealName(string name)
    {
        return MealTypeExtensions.IsValid(name);
    }
}

