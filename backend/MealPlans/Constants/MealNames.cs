namespace inzynierka.MealPlans.Constants;

public static class MealNames
{
    public const string Breakfast = "Śniadanie";
    public const string Lunch = "Obiad";
    public const string Dinner = "Kolacja";
    public const string Snack = "Przekąska";
    
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

