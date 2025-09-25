namespace inzynierka.API.Product.Model.Tag.CountryTag;

public class CountryTag: ITagEntity
{
    public int Id { get; set; }
    public string Name { get; set; }

    public ICollection<ProductCountryTag> ProductCountryTags { get; set; }
}