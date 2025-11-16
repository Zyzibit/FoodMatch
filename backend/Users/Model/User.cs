using inzynierka.MealPlans.Model;
using inzynierka.Receipts.Model;
using Microsoft.AspNetCore.Identity;

namespace inzynierka.Users.Model;

public class User : IdentityUser {

    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    //own model
    public FoodPreferences FoodPreferences { get; set; } = new FoodPreferences();
    
    public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
    public IEnumerable<MealPlan>? MealPlans { get; set; }
}
public class FoodPreferences {
    public bool IsVegan { get; set; } = false;
    public bool IsVegetarian { get; set; }= false;
    public bool HasGlutenIntolerance { get; set; } = false;
    public bool HasLactoseIntolerance { get; set; } = false;
    public List<string> Allergies { get; set; } = new();

    public int? Age { get; set; }
    public Gender? Gender { get; set; }
    public decimal? Weight { get; set; } // kg
    public decimal? Height { get; set; } // cm
    public PhysicalActivityLevel? ActivityLevel { get; set; }
    
    public int DailyProteinGoal { get; set; }
    public int DailyCarbohydrateGoal { get; set; }
    public int DailyFatGoal { get; set; }
    public int DailyCalorieGoal { get; set; }
    
    public int BreakfastCaloriePercentage { get; set; } = 30;
    public int LunchCaloriePercentage { get; set; } = 40;
    public int DinnerCaloriePercentage { get; set; } = 25;
    public int SnackCaloriePercentage { get; set; } = 5;
}

public enum Gender
{
    Male,
    Female,
    Other
}

public enum PhysicalActivityLevel
{
    Sedentary,      
    LightlyActive, 
    ModeratelyActive,
    VeryActive,
    ExtraActive
}