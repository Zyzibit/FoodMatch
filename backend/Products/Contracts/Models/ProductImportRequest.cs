namespace inzynierka.Products.Contracts.Models;

public class ProductImportRequest
{
    public string FilePath { get; set; } = string.Empty;
    public int MaxProducts { get; set; } = 100000;
    public int BatchSize { get; set; } = 1000;
}