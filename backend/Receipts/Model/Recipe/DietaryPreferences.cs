namespace inzynierka.Receipts.Model.Recipe;

public class DietaryPreferences
{
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsLactoseFree { get; set; } 
    public List<string> Allergies { get; set; } = new();
    public List<string> DislikedIngredients { get; set; } = new();
    public string? CuisineType { get; set; }
    public int? MaxCalories { get; set; }
    
    public int? DailyCalorieGoal { get; set; }
    public int? DailyProteinGoal { get; set; }
    public int? DailyCarbohydrateGoal { get; set; }
    public int? DailyFatGoal { get; set; }
    
    public string? MealType { get; set; } // "Breakfast", "Lunch", "Dinner", "Snack"
    public int? TargetMealCalories { get; set; } // Docelowe kalorie dla tego konkretnego posiłku
    public int? TargetMealProtein { get; set; } // Docelowe białko dla tego konkretnego posiłku (g)
    public int? TargetMealCarbohydrates { get; set; } // Docelowe węglowodany dla tego konkretnego posiłku (g)
    public int? TargetMealFat { get; set; } // Docelowe tłuszcze dla tego konkretnego posiłku (g)
}

