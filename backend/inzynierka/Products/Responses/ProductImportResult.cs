namespace inzynierka.Products.Responses;

public class ProductImportResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ImportedCount { get; set; }
    public int FailedCount { get; set; }
    public List<string> Warnings { get; set; } = new();
}
