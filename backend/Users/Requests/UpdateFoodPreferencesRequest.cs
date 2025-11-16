using inzynierka.Users.Model;

namespace inzynierka.Users.Requests;

public class UpdateFoodPreferencesRequest {
    public bool? IsVegan { get; set; }
    public bool? IsVegetarian { get; set; }
    public bool? HasGlutenIntolerance { get; set; }
    public bool? HasLactoseIntolerance { get; set; }
    public List<string>? Allergies { get; set; }
    
    public int? Age { get; set; }
    public string? Gender { get; set; } // "Male", "Female", "Other"
    public decimal? Weight { get; set; } // kg
    public decimal? Height { get; set; } // cm
    public string? ActivityLevel { get; set; } // "Sedentary", "LightlyActive", "ModeratelyActive", "VeryActive", "ExtraActive"
    
    public int? DailyProteinGoal { get; set; }
    public int? DailyCarbohydrateGoal { get; set; }
    public int? DailyFatGoal { get; set; }
    public int? DailyCalorieGoal { get; set; }
    
    public int? BreakfastCaloriePercentage { get; set; }
    public int? LunchCaloriePercentage { get; set; }
    public int? DinnerCaloriePercentage { get; set; }
    public int? SnackCaloriePercentage { get; set; }
}