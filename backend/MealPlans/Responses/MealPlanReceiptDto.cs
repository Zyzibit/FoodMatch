namespace inzynierka.MealPlans.Responses;

public class MealPlanReceiptDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal CaloriesPer100G { get; set; }
    public decimal ProteinPer100G { get; set; }
    public decimal CarbohydratesPer100G { get; set; }
    public decimal FatsPer100G { get; set; }
    public int Servings { get; set; }
    public int PreparationTimeMinutes { get; set; }
}

