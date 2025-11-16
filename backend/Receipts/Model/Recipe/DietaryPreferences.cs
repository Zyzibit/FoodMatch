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
    
    public int? TargetMealCalories { get; set; } // Docelowe kalorie dla tego konkretnego posiłku
    public string? MealType { get; set; } // "Breakfast", "Lunch", "Dinner", "Snack"
    public int? DailyCalorieGoal { get; set; }
    public int? DailyProteinGoal { get; set; }
    public int? DailyCarbohydrateGoal { get; set; }
    public int? DailyFatGoal { get; set; }
}

