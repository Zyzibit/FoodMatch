using inzynierka.Receipts.Model;

namespace inzynierka.Receipts.Extensions.Builders;

public class ReceiptBuilder
{
    private string? _userId;
    private ReceiptSource _source = ReceiptSource.User;
    private readonly List<ReceiptIngredient> _ingredients = new();
    private List<string>? _additionalProducts;
    private string? _title;
    private string? _description;
    private string? _instructions;
    private int _servings;
    private int _preparationTimeMinutes;
    private int _totalWeightGrams;
    private decimal? _calories;
    private decimal? _protein;
    private decimal? _carbohydrates;
    private decimal? _fats;
    private DateTime _createdAt = DateTime.UtcNow;

    public static ReceiptBuilder Create() => new ReceiptBuilder();

    public ReceiptBuilder ForUser(string userId)
    {
        _userId = userId;
        return this;
    }

    public ReceiptBuilder FromSource(ReceiptSource source)
    {
        _source = source;
        return this;
    }

    public ReceiptBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public ReceiptBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public ReceiptBuilder WithInstructions(string instructions)
    {
        _instructions = instructions;
        return this;
    }

    public ReceiptBuilder WithServings(int servings)
    {
        _servings = servings;
        return this;
    }

    public ReceiptBuilder WithPreparationTime(int minutes)
    {
        _preparationTimeMinutes = minutes;
        return this;
    }

    public ReceiptBuilder WithTotalWeightGrams(int grams)
    {
        _totalWeightGrams = grams;
        return this;
    }

    public ReceiptBuilder WithMacros(decimal calories, decimal protein, decimal carbohydrates, decimal fats)
    {
        _calories = calories;
        _protein = protein;
        _carbohydrates = carbohydrates;
        _fats = fats;
        return this;
    }

    public ReceiptBuilder WithAdditionalProducts(IEnumerable<string>? products)
    {
        _additionalProducts = products?.ToList();
        return this;
    }

    public ReceiptBuilder AddAdditionalProduct(string product)
    {
        _additionalProducts ??= new List<string>();
        _additionalProducts.Add(product);
        return this;
    }

    public ReceiptBuilder WithIngredients(IEnumerable<ReceiptIngredient> ingredients)
    {
        _ingredients.Clear();
        _ingredients.AddRange(ingredients);
        return this;
    }

    public ReceiptBuilder AddIngredient(ReceiptIngredient ingredient)
    {
        _ingredients.Add(ingredient);
        return this;
    }

    public ReceiptBuilder AddIngredient(int productId, int unitId, decimal quantity, decimal? normalizedQuantityInGrams = null)
    {
        _ingredients.Add(new ReceiptIngredient
        {
            ProductId = productId,
            UnitId = unitId,
            Quantity = quantity,
            NormalizedQuantityInGrams = normalizedQuantityInGrams
        });
        return this;
    }

    public ReceiptBuilder CreatedAt(DateTime utc)
    {
        _createdAt = utc;
        return this;
    }

    public Receipt Build()
    {
        if (string.IsNullOrWhiteSpace(_userId))
            throw new InvalidOperationException("UserId is required");
        if (string.IsNullOrWhiteSpace(_title) || _title.Length < 3)
            throw new InvalidOperationException("Title is required and must be at least 3 characters long");
        if (string.IsNullOrWhiteSpace(_instructions))
            throw new InvalidOperationException("Instructions are required");
        if (_totalWeightGrams <= 0)
            throw new InvalidOperationException("TotalWeightGrams must be greater than 0");
        if (!_calories.HasValue || !_protein.HasValue || !_carbohydrates.HasValue || !_fats.HasValue)
            throw new InvalidOperationException("Macros (calories, protein, carbohydrates, fats) are required");

        
        
        return new Receipt
        {
            UserId = _userId,
            Source = _source,
            Ingredients = _ingredients,
            AdditionalProducts = _additionalProducts,
            Title = _title,
            Description = _description ?? string.Empty,
            Instructions = _instructions,
            Servings = _servings,
            PreparationTimeMinutes = _preparationTimeMinutes,
            TotalWeightGrams = _totalWeightGrams,
            Calories = _calories.Value,
            Protein = _protein.Value,
            Carbohydrates = _carbohydrates.Value,
            Fats = _fats.Value,
            CreatedAt = _createdAt
        };
    }
}
