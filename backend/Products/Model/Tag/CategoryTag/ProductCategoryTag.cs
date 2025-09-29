namespace inzynierka.Products.Model.Tag.CategoryTag;

public class ProductCategoryTag
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CategoryTagId { get; set; }
    public CategoryTag CategoryTag { get; set; }
}
