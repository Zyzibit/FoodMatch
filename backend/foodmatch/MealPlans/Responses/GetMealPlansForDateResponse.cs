namespace inzynierka.MealPlans.Responses;

public class GetMealPlansForDateResponse
{
    public bool Success { get; set; }
    public List<MealPlanDto> MealPlans { get; set; } = new();
    public decimal TotalCalories { get; set; }
    public decimal TotalProteins { get; set; }
    public decimal TotalCarbohydrates { get; set; }
    public decimal TotalFats { get; set; }
    public string? Message { get; set; }
}

