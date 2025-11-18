using inzynierka.Recipes.Model;

namespace inzynierka.Recipes.Builders;

public class RecipeBuilder
{
    private string? _userId;
    private RecipeSource _source = RecipeSource.User;
    private readonly List<RecipeIngredient> _ingredients = new();
    private List<string>? _additionalProducts;
    private string? _title;
    private string? _description;
    private string? _instructions;
    private int _preparationTimeMinutes;
    private int _totalWeightGrams;
    private decimal? _calories;
    private decimal? _protein;
    private decimal? _carbohydrates;
    private decimal? _fats;
    private DateTime _createdAt = DateTime.UtcNow;

    public static RecipeBuilder Create() => new RecipeBuilder();

    public RecipeBuilder ForUser(string userId)
    {
        _userId = userId;
        return this;
    }

    public RecipeBuilder FromSource(RecipeSource source)
    {
        _source = source;
        return this;
    }

    public RecipeBuilder WithTitle(string title)
    {
        _title = title;
        return this;
    }

    public RecipeBuilder WithDescription(string? description)
    {
        _description = description;
        return this;
    }

    public RecipeBuilder WithInstructions(string instructions)
    {
        _instructions = instructions;
        return this;
    }

    public RecipeBuilder WithPreparationTime(int minutes)
    {
        _preparationTimeMinutes = minutes;
        return this;
    }

    public RecipeBuilder WithTotalWeightGrams(int grams)
    {
        _totalWeightGrams = grams;
        return this;
    }

    public RecipeBuilder WithMacros(decimal calories, decimal protein, decimal carbohydrates, decimal fats)
    {
        _calories = calories;
        _protein = protein;
        _carbohydrates = carbohydrates;
        _fats = fats;
        return this;
    }

    public RecipeBuilder WithAdditionalProducts(IEnumerable<string>? products)
    {
        _additionalProducts = products?.ToList();
        return this;
    }

    public RecipeBuilder AddAdditionalProduct(string product)
    {
        _additionalProducts ??= new List<string>();
        _additionalProducts.Add(product);
        return this;
    }

    public RecipeBuilder WithIngredients(IEnumerable<RecipeIngredient> ingredients)
    {
        _ingredients.Clear();
        _ingredients.AddRange(ingredients);
        return this;
    }

    public RecipeBuilder AddIngredient(RecipeIngredient ingredient)
    {
        _ingredients.Add(ingredient);
        return this;
    }

    public RecipeBuilder AddIngredient(int productId, int unitId, decimal quantity, decimal? normalizedQuantityInGrams = null)
    {
        _ingredients.Add(new RecipeIngredient
        {
            ProductId = productId,
            UnitId = unitId,
            Quantity = quantity,
            NormalizedQuantityInGrams = normalizedQuantityInGrams
        });
        return this;
    }

    public RecipeBuilder CreatedAt(DateTime utc)
    {
        _createdAt = utc;
        return this;
    }

    public Recipe Build()
    {
        if (string.IsNullOrWhiteSpace(_userId))
            throw new InvalidOperationException("UserId is required");
        if (string.IsNullOrWhiteSpace(_title) || _title.Length < 3)
            throw new InvalidOperationException("Title is required and must be at least 3 characters long");
        if (string.IsNullOrWhiteSpace(_instructions))
            throw new InvalidOperationException("Instructions are required");
        if (!_calories.HasValue || !_protein.HasValue || !_carbohydrates.HasValue || !_fats.HasValue)
            throw new InvalidOperationException("Macros (calories, protein, carbohydrates, fats) are required");
        
        if (_totalWeightGrams <= 0)
        {
            _totalWeightGrams = (int)Math.Round(_ingredients.Sum(i => i.NormalizedQuantityInGrams ?? 0));
            
            if (_totalWeightGrams <= 0)
            {
                throw new InvalidOperationException("TotalWeightGrams must be greater than 0. Ensure ingredients have NormalizedQuantityInGrams values.");
            }
        }
        
        return new Recipe
        {
            UserId = _userId,
            Source = _source,
            Ingredients = _ingredients,
            AdditionalProducts = _additionalProducts,
            Title = _title,
            Description = _description ?? string.Empty,
            Instructions = _instructions,
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
