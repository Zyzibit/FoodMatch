using inzynierka.Products.Model;

namespace inzynierka.Products.Repositories
{
    /// <summary>
    /// Ładunek jednej paczki importu: encje produktów oraz pary (kod produktu, nazwa tagu)
    /// dla każdego rodzaju tagu. Nazwy tagów są wyprowadzane z tych par po stronie SQL,
    /// więc nie trzeba osobno utrzymywać zbiorów nazw.
    /// </summary>
    public sealed class ProductBatch
    {
        public List<Product> Products { get; }
        public List<(string Code, string TagName)> IngredientLinks { get; }
        public List<(string Code, string TagName)> CountryLinks { get; }
        public List<(string Code, string TagName)> CategoryLinks { get; }
        public List<(string Code, string TagName)> AllergenLinks { get; }

        public ProductBatch(int capacity)
        {
            Products        = new List<Product>(capacity);
            IngredientLinks = new List<(string, string)>(capacity * 3);
            CountryLinks    = new List<(string, string)>(capacity);
            CategoryLinks   = new List<(string, string)>(capacity * 2);
            AllergenLinks   = new List<(string, string)>(capacity);
        }

        public bool IsEmpty =>
            Products.Count == 0 &&
            IngredientLinks.Count == 0 && CountryLinks.Count == 0 &&
            CategoryLinks.Count == 0 && AllergenLinks.Count == 0;
    }
}
