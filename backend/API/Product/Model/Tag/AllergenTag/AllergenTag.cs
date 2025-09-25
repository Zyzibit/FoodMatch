namespace inzynierka.API.Product.Model.Tag.AllergenTag;

public class AllergenTag: ITagEntity
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection <ProductAllergenTag> ProductAllergenTags { get; set; }
}