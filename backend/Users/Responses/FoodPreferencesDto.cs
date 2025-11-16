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
    
    public int? DailyProteinGoal { get; set; }
    public int? DailyCarbohydrateGoal { get; set; }
    public int? DailyFatGoal { get; set; }
    public int? DailyCalorieGoal { get; set; }
    
    public int? BreakfastCaloriePercentage { get; set; }
    public int? LunchCaloriePercentage { get; set; }
    public int? DinnerCaloriePercentage { get; set; }
    public int? SnackCaloriePercentage { get; set; }
    
    public int? CalculatedBMR { get; set; }
    public int? CalculatedDailyCalories { get; set; }
    public int? BreakfastCalories { get; set; }
    public int? LunchCalories { get; set; }
    public int? DinnerCalories { get; set; }
    public int? SnackCalories { get; set; }
}