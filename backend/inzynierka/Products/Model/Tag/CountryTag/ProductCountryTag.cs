namespace inzynierka.Products.Model.Tag.CountryTag;

public class ProductCountryTag
{
    public int ProductId { get; set; }
    public Product Product { get; set; }

    public int CountryTagId { get; set; }
    public CountryTag CountryTag { get; set; }
}