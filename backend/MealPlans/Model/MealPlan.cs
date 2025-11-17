using inzynierka.Receipts.Extensions.Model;
using inzynierka.Users.Model;

namespace inzynierka.MealPlans.Model;

public class MealPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    
    public int? ReceiptId { get; set; } 
    public Receipt? Receipt { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    
}