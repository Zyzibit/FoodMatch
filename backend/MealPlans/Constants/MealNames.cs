namespace inzynierka.MealPlans.Constants;

public static class MealNames
{
    public const string Breakfast = "Breakfast";
    public const string Lunch = "Lunch";
    public const string Dinner = "Dinner";
    public const string Snack = "Snack";
    
    public static readonly HashSet<string> AllowedMealNames = new()
    {
        Breakfast,
        Lunch,
        Dinner,
        Snack
    };
    
    public static bool IsValidMealName(string name)
    {
        return AllowedMealNames.Contains(name);
    }
}

