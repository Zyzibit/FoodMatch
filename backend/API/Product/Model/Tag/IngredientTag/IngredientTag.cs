namespace inzynierka.API.Product.Model.Tag.IngredientTag;

public class IngredientTag: ITagEntity
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<ProductIngredientTag> ProductIngredientTags { get; set; }
}
