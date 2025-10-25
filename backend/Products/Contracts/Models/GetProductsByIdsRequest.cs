namespace inzynierka.Products.Contracts.Models;

public class GetProductsByIdsRequest
{
    public List<int> ProductIds { get; set; } = new();
}
