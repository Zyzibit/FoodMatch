namespace inzynierka.API.Product.Model.Tag.CategoryTag;

public class ProductCategoryTag
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CategoryTagId { get; set; }
    public Tag.CategoryTag.CategoryTag CategoryTag { get; set; }
}
