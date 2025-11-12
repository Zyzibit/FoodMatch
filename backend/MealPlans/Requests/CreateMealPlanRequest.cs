namespace inzynierka.MealPlans.Requests;

public class CreateMealPlanRequest
{ 
    public string MealName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public int? ReceiptId { get; set; }
}