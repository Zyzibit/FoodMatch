namespace inzynierka.API.Product.Model.Tag.AllergenTag;

public class ProductAllergenTag
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int AllergenTagId { get; set; }
    public Tag.AllergenTag.AllergenTag AllergenTag { get; set; }
}