using inzynierka.MealPlans.Model;
using inzynierka.Recipes.Model;
using Microsoft.AspNetCore.Identity;

namespace inzynierka.Users.Model;

public class User : IdentityUser {

    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    //own model
    public FoodPreferences FoodPreferences { get; set; } = new FoodPreferences();
    
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
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
    public FitnessGoal? FitnessGoal { get; set; }
    
    public int DailyProteinGoal { get; set; }
    public int DailyCarbohydrateGoal { get; set; }
    public int DailyFatGoal { get; set; }
    public int DailyCalorieGoal { get; set; }
    
    public MealNutritionDistribution Breakfast { get; set; } = new() 
    { 
        CaloriePercentage = 30, 
        ProteinPercentage = 25, 
        CarbohydratePercentage = 30, 
        FatPercentage = 30 
    };
    
    public MealNutritionDistribution Lunch { get; set; } = new() 
    { 
        CaloriePercentage = 40, 
        ProteinPercentage = 35, 
        CarbohydratePercentage = 40, 
        FatPercentage = 40 
    };
    
    public MealNutritionDistribution Dinner { get; set; } = new() 
    { 
        CaloriePercentage = 25, 
        ProteinPercentage = 35, 
        CarbohydratePercentage = 25, 
        FatPercentage = 25 
    };
    
    public MealNutritionDistribution Snack { get; set; } = new() 
    { 
        CaloriePercentage = 5, 
        ProteinPercentage = 5, 
        CarbohydratePercentage = 5, 
        FatPercentage = 5 
    };
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

public enum FitnessGoal
{
    WeightLoss,
    Maintenance,
    WeightGain
}

