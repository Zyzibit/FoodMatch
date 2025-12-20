namespace inzynierka.Recipes.Requests;

public class RecipeIngredientDto
{
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal NormalizedQuantityInGrams { get; set; }
    public decimal Quantity { get; set; }
}