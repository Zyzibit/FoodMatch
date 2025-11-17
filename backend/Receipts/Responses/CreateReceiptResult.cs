namespace inzynierka.Receipts.Responses;

public class CreateReceiptResult
{
    public bool Success { get; set; }
    public int? ReceiptId { get; set; }
    public string? ErrorMessage { get; set; }
}

