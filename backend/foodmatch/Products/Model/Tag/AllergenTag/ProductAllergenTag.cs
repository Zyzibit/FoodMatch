namespace inzynierka.Products.Model.Tag.AllergenTag;

public class ProductAllergenTag
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int AllergenTagId { get; set; }
    public AllergenTag AllergenTag { get; set; }
}