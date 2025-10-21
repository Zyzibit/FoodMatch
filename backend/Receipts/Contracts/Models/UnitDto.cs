namespace inzynierka.Receipts.Contracts.Models;

public class UnitDto
{
    public int UnitId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PromptDescription { get; set; } = string.Empty;
}