
namespace inzynierka.Receipts.Contracts.Models;

public class UnitOperationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public UnitDto? Unit { get; set; }
}
