namespace inzynierka.Recipes.Responses;

public class RecipeDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public List<RecipeIngredientReadDto> Ingredients { get; set; } = new List<RecipeIngredientReadDto>();
    
    public List<string>? AdditionalProducts { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int PreparationTimeMinutes { get; set; }
    public int TotalWeightGrams { get; set; }
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fats { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RecipeIngredientReadDto
{
    public int ProductId { get; set; }
    public int UnitId { get; set; }
    public decimal Quantity { get; set; }
    public decimal? NormalizedQuantityInGrams { get; set; }
    public string Source { get; set; } = string.Empty;
    public decimal Calories { get; set; }
    public decimal Proteins { get; set; }
    public decimal Carbohydrates { get; set; }
    public decimal Fats { get; set; }
    public string ProductName { get; set; } = string.Empty;
}

