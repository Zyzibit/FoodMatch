namespace inzynierka.Receipts.Extensions.Responses;

public class UnitOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public UnitDto? Unit { get; set; }
}


