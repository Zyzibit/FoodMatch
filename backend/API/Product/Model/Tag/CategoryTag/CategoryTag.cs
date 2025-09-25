
namespace inzynierka.API.Product.Model.Tag.CategoryTag;

public class CategoryTag: ITagEntity
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<ProductCategoryTag> ProductCategoryTags { get; set; }
}