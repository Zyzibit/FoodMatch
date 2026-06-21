namespace inzynierka.Products.Model.Tag.IngredientTag;

public class ProductIngredientTag
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int IngredientTagId { get; set; }
    public IngredientTag IngredientTag { get; set; }
}