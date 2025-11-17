namespace inzynierka.Receipts.Extensions.Responses;

public class ReceiptsListResult
{
    public bool Success { get; set; }
    public List<ReceiptDto> Receipts { get; set; } = new();
    public int TotalCount { get; set; }
    public string? ErrorMessage { get; set; }
}

