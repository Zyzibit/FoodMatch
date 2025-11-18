using inzynierka.Products.Model;
using inzynierka.Units.Models;

namespace inzynierka.Recipes.Model;

public class RecipeIngredient {
    public int RecipeId { get; set; }
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
    
    
    public decimal? NormalizedQuantityInGrams { get; set; }
    
    public Unit Unit { get; set; }
    public Recipe Recipe { get; set; }
    public Product Product { get; set; }
    
}