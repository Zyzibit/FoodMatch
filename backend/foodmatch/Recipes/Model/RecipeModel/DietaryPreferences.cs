namespace inzynierka.Recipes.Model.RecipeModel;

public class DietaryPreferences
{
    public bool IsVegetarian { get; set; }
    public bool IsVegan { get; set; }
    public bool IsGlutenFree { get; set; }
    public bool IsLactoseFree { get; set; } 
    public List<string> Allergies { get; set; } = new();
    public List<string> DislikedIngredients { get; set; } = new();
    
    public string? FitnessGoal { get; set; } // "WeightLoss", "Maintenance", "WeightGain"
    
    public int? DailyCalorieGoal { get; set; }
    public int? DailyProteinGoal { get; set; }
    public int? DailyCarbohydrateGoal { get; set; }
    public int? DailyFatGoal { get; set; }
    
    public int? TargetMealCalories { get; set; }
    public int? TargetMealProtein { get; set; } 
    public int? TargetMealCarbohydrates { get; set; }
    public int? TargetMealFat { get; set; } 
}

