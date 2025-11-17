namespace inzynierka.Receipts.Extensions.Requests;

public class CreateUnitRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PromptDescription { get; set; } = string.Empty;
}

