namespace inzynierka.MealPlans.Responses;

public class GetMealPlansResponse
{
    public bool Success { get; set; }
    public List<MealPlanDto> MealPlans { get; set; } = new();
    public string? Message { get; set; }
}

