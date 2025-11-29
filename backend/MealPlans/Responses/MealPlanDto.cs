namespace inzynierka.MealPlans.Responses;

public class MealPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public MealPlanRecipeDto Recipe { get; set; }
}

