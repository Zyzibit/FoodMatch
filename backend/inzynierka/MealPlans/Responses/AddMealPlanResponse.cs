namespace inzynierka.MealPlans.Responses;

public class AddMealPlanResponse
{
    public bool Success { get; set; }
    public int? MealPlanId { get; set; }
    public string? Message { get; set; }
}

