namespace inzynierka.Users.Contracts.Models;

public class UpdateFoodPreferencesRequest {
    public bool IsVegan { get; set; }
    public bool IsVegetarian { get; set; }
    public bool HasGlutenIntolerance { get; set; }
    public bool HasLactoseIntolerance { get; set; }
    public bool HasNutAllergy { get; set; }
    
    public int DailyProteinGoal { get; set; }
    public int DailyCarbohydrateGoal { get; set; }
    public int DailyFatGoal { get; set; }
    public int DailyCalorieGoal { get; set; }
}