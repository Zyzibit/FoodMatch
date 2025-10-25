using System.Collections.Generic;

namespace inzynierka.Products.Contracts.Models;

public class GetProductsByIdsResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public IEnumerable<ProductInfo> Products { get; set; } = new List<ProductInfo>();
}
