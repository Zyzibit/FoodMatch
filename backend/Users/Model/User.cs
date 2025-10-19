using Microsoft.AspNetCore.Identity;

namespace inzynierka.Users.Model;

public class User : IdentityUser {

    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    //own model
    public FoodPreferences FoodPreferences { get; set; } = new FoodPreferences();

}
public class FoodPreferences {
    public bool IsVegan { get; set; } = false;
    public bool IsVegetarian { get; set; }= false;
    public bool HasGlutenIntolerance { get; set; } = false;
    public bool HasLactoseIntolerance { get; set; } = false;
    public bool HasNutAllergy { get; set; } = false;

    public int DailyProteinGoal { get; set; }
    public int DailyCarbohydrateGoal { get; set; }
    public int DailyFatGoal { get; set; }
    public int DailyCalorieGoal { get; set; }
}