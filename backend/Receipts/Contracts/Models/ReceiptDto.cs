using System.Collections.Generic;

namespace inzynierka.Receipts.Contracts.Models;

public class ReceiptDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public bool IsAiGenerated { get; set; }
    public List<ReceiptIngredientReadDto> Ingredients { get; set; } = new List<ReceiptIngredientReadDto>();
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
    public DateTime CreatedAt { get; set; }
}

public class ReceiptIngredientReadDto
{
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
}

public class ReceiptsListResult
{
    public bool Success { get; set; } = true;
    public List<ReceiptDto> Receipts { get; set; } = new List<ReceiptDto>();
    public int TotalCount { get; set; }
}

