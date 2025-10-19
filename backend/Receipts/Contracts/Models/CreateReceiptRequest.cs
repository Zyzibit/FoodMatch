using System.Collections.Generic;

namespace inzynierka.Receipts.Contracts.Models;

public class CreateReceiptRequest
{
    public List<ReceiptIngredientDto> Ingredients { get; set; } = new List<ReceiptIngredientDto>();
    public List<string>? AdditionalProducts { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int Servings { get; set; }
    public int PreparationTimeMinutes { get; set; }
    public int Calories { get; set; }
    public int Protein { get; set; }
    public int Carbohydrates { get; set; }
    public int Fats { get; set; }
}

public class ReceiptIngredientDto
{
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
}

