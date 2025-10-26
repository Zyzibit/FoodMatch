namespace inzynierka.Receipts.Requests;

public class CreateReceiptRequest
{
    public List<ReceiptIngredientDto> Ingredients { get; set; } = new List<ReceiptIngredientDto>();
    public List<string>? AdditionalProducts { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Servings { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public int TotalWeightGrams { get; set; }
    public int CaloriesPer100G { get; set; }
    public int ProteinPer100G { get; set; }
    public int CarbohydratesPer100G { get; set; }
    public int FatsPer100G { get; set; }
}

public class ReceiptIngredientDto
{
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
}
