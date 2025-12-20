namespace inzynierka.Users.Responses;

public class MealNutritionDto
{
    public int? CaloriePercentage { get; set; }
    public int? ProteinPercentage { get; set; }
    public int? CarbohydratePercentage { get; set; }
    public int? FatPercentage { get; set; }
    
    public int? CaloriesGoal { get; set; }
    public int? ProteinGoal { get; set; }
    public int? CarbohydrateGoal { get; set; }
    public int? FatGoal { get; set; }
}

