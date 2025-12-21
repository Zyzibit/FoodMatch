using System.Text.Json.Serialization;

namespace inzynierka.MealPlans.Requests;

public class CreateMealPlanRequest
{ 
    public string MealName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int? RecipeId { get; set; }
    public decimal ServingMultiplier { get; set; } = 1.0m;
}