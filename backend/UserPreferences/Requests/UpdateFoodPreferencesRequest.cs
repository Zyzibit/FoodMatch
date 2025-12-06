namespace inzynierka.UserPreferences.Requests;

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
    public string? FitnessGoal { get; set; } // "WeightLoss", "Maintenance", "WeightGain"
    
    public int? DailyProteinGoal { get; set; }
    public int? DailyCarbohydrateGoal { get; set; }
    public int? DailyFatGoal { get; set; }
    public int? DailyCalorieGoal { get; set; }
    
    public UpdateMealNutritionDistributionRequest? Breakfast { get; set; }
    public UpdateMealNutritionDistributionRequest? Lunch { get; set; }
    public UpdateMealNutritionDistributionRequest? Dinner { get; set; }
    public UpdateMealNutritionDistributionRequest? Snack { get; set; }
}


