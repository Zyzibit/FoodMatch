using inzynierka.Recipes.Model;
using inzynierka.Users.Model;

namespace inzynierka.MealPlans.Model;

public class MealPlan
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    
    public int? RecipeId { get; set; } 
    public Recipe? Recipe { get; set; }
    
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    
}