namespace inzynierka.Products.OpenFoodFacts.Import;

public interface IProductImporter {
    Task ImportAsync(string path, int maxProducts);
}