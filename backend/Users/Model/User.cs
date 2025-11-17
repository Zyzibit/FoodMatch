using inzynierka.MealPlans.Model;
using inzynierka.Receipts.Extensions.Model;
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
    
    // Cele dzienne - suma wszystkich posiłków
    public int DailyProteinGoal { get; set; }
    public int DailyCarbohydrateGoal { get; set; }
    public int DailyFatGoal { get; set; }
    public int DailyCalorieGoal { get; set; }
    
    // Procentowy rozkład kalorii na poszczególne posiłki
    public int BreakfastCaloriePercentage { get; set; } = 30;
    public int LunchCaloriePercentage { get; set; } = 40;
    public int DinnerCaloriePercentage { get; set; } = 25;
    public int SnackCaloriePercentage { get; set; } = 5;

    // Procentowy rozkład białka (domyślnie równomierny)
    public int BreakfastProteinPercentage { get; set; } = 25;
    public int LunchProteinPercentage { get; set; } = 35;
    public int DinnerProteinPercentage { get; set; } = 35;
    public int SnackProteinPercentage { get; set; } = 5;

    // Procentowy rozkład węglowodanów
    public int BreakfastCarbohydratePercentage { get; set; } = 30;
    public int LunchCarbohydratePercentage { get; set; } = 40;
    public int DinnerCarbohydratePercentage { get; set; } = 25;
    public int SnackCarbohydratePercentage { get; set; } = 5;

    // Procentowy rozkład tłuszczów
    public int BreakfastFatPercentage { get; set; } = 30;
    public int LunchFatPercentage { get; set; } = 40;
    public int DinnerFatPercentage { get; set; } = 25;
    public int SnackFatPercentage { get; set; } = 5;
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