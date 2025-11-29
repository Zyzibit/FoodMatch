namespace inzynierka.Users.Responses;

public class FoodPreferencesDto {
    public bool? IsVegan { get; set; }
    public bool? IsVegetarian { get; set; }
    public bool? HasGlutenIntolerance { get; set; }
    public bool? HasLactoseIntolerance { get; set; }
    public List <string>? Allergies { get; set; }
    
    public int? Age { get; set; }
    public string? Gender { get; set; }
    public decimal? Weight { get; set; }
    public decimal? Height { get; set; }
    public string? ActivityLevel { get; set; }
    public string? FitnessGoal { get; set; }
    public int? DailyProteinGoal { get; set; }
    public int? DailyCarbohydrateGoal { get; set; }
    public int? DailyFatGoal { get; set; }
    public int? DailyCalorieGoal { get; set; }
    public int? CalculatedBMR { get; set; }
    public int? CalculatedDailyCalories { get; set; }
    
    public MealNutritionDto? Breakfast { get; set; }
    public MealNutritionDto? Lunch { get; set; }
    public MealNutritionDto? Dinner { get; set; }
    public MealNutritionDto? Snack { get; set; }
}