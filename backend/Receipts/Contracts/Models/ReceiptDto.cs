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
    public int TotalWeightGrams { get; set; }
    public decimal CaloriesPer100G { get; set; }
    public decimal ProteinPer100G { get; set; }
    public decimal CarbohydratesPer100G { get; set; }
    public decimal FatsPer100G { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ReceiptIngredientReadDto
{
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
    public bool IsAiGenerated { get; set; }
    public string ProductName { get; set; } = string.Empty;
    
    public decimal EstimatedCalories { get; set; }
    public decimal EstimatedProteins { get; set; }
    public decimal EstimatedCarbohydrates { get; set; }
    public decimal EstimatedFats { get; set; }
    
}

public class ReceiptsListResult
{
    public bool Success { get; set; } = true;
    public List<ReceiptDto> Receipts { get; set; } = new List<ReceiptDto>();
    public int TotalCount { get; set; }
}
