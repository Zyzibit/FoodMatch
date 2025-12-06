using inzynierka.Products.Model;
using inzynierka.Products.Services;
using inzynierka.Recipes.Builders;
using inzynierka.Recipes.Extensions;
using inzynierka.Recipes.Model;
using inzynierka.Recipes.Repositories;
using inzynierka.Recipes.Requests;
using inzynierka.Recipes.Responses;
using inzynierka.Units.Services;
using inzynierka.Users.Services;
using inzynierka.UserPreferences.Services;

namespace inzynierka.Recipes.Services;


public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ILogger<RecipeService> _logger;
    private readonly IRecipeGeneratorService _recipeGeneratorService;
    private readonly IProductService _productService;
    private readonly IUnitService _unitService;
    private readonly IUserService _userService;
    private readonly IUserPreferencesService _userPreferencesService;

    public RecipeService(
        IRecipeRepository recipeRepository, 
        ILogger<RecipeService> logger,
        IUserService userService,
        IUserPreferencesService userPreferencesService,
        IRecipeGeneratorService recipeGeneratorService,
        IProductService productService,
        IUnitService unitService
)
    {
        _recipeRepository = recipeRepository;
        _logger = logger;
        _userService = userService;
        _userPreferencesService = userPreferencesService;
        _recipeGeneratorService = recipeGeneratorService;
        _productService = productService;
        _unitService = unitService;
    }
    public async Task<RecipeDto?> GetRecipeAsync(int id)
    {
        var recipe = await _recipeRepository.GetRecipeByIdAsync(id);
        if (recipe == null) return null;

        return recipe.ToDto();
    }

    public async Task<RecipeListResult> Recipes(int limit = 50, int offset = 0)
    {
        var (recipes, total) = await _recipeRepository.GetAllRecipesAsync(limit, offset);
        var dtoList = recipes.ToDtoList().ToList();
        return new RecipeListResult { Success = true, Recipes = dtoList, TotalCount = total };
    }

    public async Task<RecipeListResult> GetUserRecipesAsync(string userId, int limit = 50, int offset = 0)
    {
        var (recipes, total) = await _recipeRepository.GetUserRecipesAsync(userId, limit, offset);
        var dtoList = recipes.ToDtoList().ToList();
        return new RecipeListResult { Success = true, Recipes = dtoList, TotalCount = total };
    }

    public async Task<CreateRecipeResult> CreateRecipeAsync(string userId, CreateRecipeRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID is required to create a recipe", nameof(userId));
            }

            var userExists = await _userService.GetUserByIdAsync(userId);
            if (userExists == null)
            {
                throw new InvalidOperationException($"User with ID {userId} does not exist in the database");
            }
            
            var ingredients = request.Ingredients.Select(i => new RecipeIngredient
            {
                ProductId = i.ProductId,
                UnitId = i.UnitId,
                Quantity = i.Quantity,
                NormalizedQuantityInGrams = i.NormalizedQuantityInGrams
            }).ToList();

            var builder = RecipeBuilder.Create()
                .ForUser(userId)
                .FromSource(RecipeSource.User)
                .WithIngredients(ingredients)
                .WithAdditionalProducts(request.AdditionalProducts)
                .WithTitle(request.Title)
                .WithDescription(request.Description)
                .WithInstructions(request.Instructions)
                .WithPreparationTime(request.PreparationTimeMinutes)
                .WithTotalWeightGrams(request.TotalWeightGrams)
                .WithMacros(
                    calories: request.Calories,
                    protein: request.Proteins,
                    carbohydrates: request.Carbohydrates,
                    fats: request.Fats)
                .CreatedAt(DateTime.UtcNow);

            var recipe = builder.Build();

            var result = await _recipeRepository.AddRecipeAsync(recipe);
            return new CreateRecipeResult { Success = true, RecipeId = result.Id };
        }
        catch (Exception ex)
        {
            return new CreateRecipeResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<GenerateRecipePreviewResult> GenerateRecipePreviewAsync(string userId, GenerateRecipeRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID is required to generate a recipe", nameof(userId));
            }

            var userExists = await _userService.GetUserByIdAsync(userId);
            if (userExists == null)
            {
                throw new InvalidOperationException($"User with ID {userId} does not exist in the database");
            }
            
            var userPreferences = await _userPreferencesService.GetUserFoodPreferencesAsync(userId);
            
            var preferences = request.Preferences.MergeWithUserPreferences(userPreferences);
            preferences.ApplyMealTypeGoals(request.MealType, userPreferences);
            var productsInfo = await _productService.GetProductsByIdsAsync(request.ProductIds);
            var productsList = productsInfo.ToList();
            
            var ingredientNames = new List<string>();

            // if (productsList.Any())
            // {
            //     ingredientNames.AddRange(productsList.Select(p => _productService.GetProductDisplayName(p)));
            // }

            if (request.AvailableIngredients.Any())
            {
                ingredientNames.AddRange(request.AvailableIngredients);
            }
            request.AvailableIngredients = ingredientNames;
            request.Preferences = preferences;


            var aiResult = await _recipeGeneratorService.GenerateRecipeAsync(request);

            if (!aiResult.Success || aiResult.Recipe == null)
            {
                return new GenerateRecipePreviewResult 
                { 
                    Success = false, 
                    ErrorMessage = aiResult.ErrorMessage ?? "Failed to generate recipe with AI" 
                };
            }

            var generatedRecipe = aiResult.Recipe;
            
            var productDtoList = generatedRecipe.Ingredients.MapToProductDtoList();
            var usedProducts = _productService.GetMatchingProducts(productsList, productDtoList);
            
            var units = await _unitService.GetAllUnitsAsync();
            
            var previewIngredients = new List<PreviewRecipeIngredientDto>();

            var distinctProducts = usedProducts.GroupBy(p => p.Id).Select(g => g.First()).ToList();
            foreach (var product in distinctProducts)
            {
                var productName = _productService.GetProductDisplayName(product);
                var matchingIngredient = _productService.FindMatchingProduct(product, productDtoList);
                
                if (matchingIngredient == null || !int.TryParse(product.Id, out var productId))
                    continue;

                var aiIngredient = generatedRecipe.Ingredients.FirstOrDefault(ai => ai.Name == matchingIngredient.Name);
                if (aiIngredient == null)
                    continue;

                var unit = units.FirstOrDefault(u => 
                    u.Name.Equals(aiIngredient.Unit?.Trim(), StringComparison.OrdinalIgnoreCase));
                
                var unitId = unit?.UnitId ?? units.FirstOrDefault(u => 
                    u.Name.Equals("gram", StringComparison.OrdinalIgnoreCase))?.UnitId ?? 1;

                if (unit?.Name != null)
                {
                    previewIngredients.Add(aiIngredient.ToPreviewIngredientDto(
                        productId, productName, unitId, unit.Name, aiIngredient.Quantity, ProductSource.User));
                }
            }

            var additionalIngredientsData = generatedRecipe.Ingredients.Where(ai => !ai.ProductId.HasValue).ToList();
            var additionalProductsList = new List<string>();

            foreach (var aiIngredient in additionalIngredientsData)
            {
                try
                {
                    var aiProduct = await _productService.CreateAiGeneratedProductAsync(aiIngredient);
                    var unit = units.FirstOrDefault(u => 
                        u.Name.Equals(aiIngredient.Unit?.Trim(), StringComparison.OrdinalIgnoreCase));
                    
                    var unitId = unit?.UnitId ?? units.FirstOrDefault(u => 
                        u.Name.Equals("gram", StringComparison.OrdinalIgnoreCase))?.UnitId ?? 1;
                    
                    var unitName = unit?.Name ?? aiIngredient.Unit ?? "gram";

                    previewIngredients.Add(aiIngredient.ToPreviewIngredientDto(
                        aiProduct.Id, aiIngredient.Name, unitId, unitName, aiIngredient.Quantity, ProductSource.AI));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to process AI ingredient {Name}, adding to additional products", aiIngredient.Name);
                    additionalProductsList.Add($"{aiIngredient.Name} ({aiIngredient.Quantity} {aiIngredient.Unit})");
                }
            }
            
            var preview = generatedRecipe.ToPreviewDto(previewIngredients, additionalProductsList);

            return new GenerateRecipePreviewResult
            {
                Success = true,
                Recipe = preview
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recipe preview with AI");
            return new GenerateRecipePreviewResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task<CreateRecipeResult> SaveGeneratedRecipeAsync(string userId, SaveGeneratedRecipeRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException("User ID is required to save a recipe", nameof(userId));
            }

            var userExists = await _userService.GetUserByIdAsync(userId);
            if (userExists == null)
            {
                throw new InvalidOperationException($"User with ID {userId} does not exist in the database");
            }

            var ingredients = request.Ingredients.Select(i => new RecipeIngredient
            {
                ProductId = i.ProductId,
                UnitId = i.UnitId,
                Quantity = i.Quantity,
                NormalizedQuantityInGrams = i.NormalizedQuantityInGrams
            }).ToList();

            var builder = RecipeBuilder.Create()
                .ForUser(userId)
                .FromSource(RecipeSource.AI)
                .WithIngredients(ingredients)
                .WithAdditionalProducts(request.AdditionalProducts)
                .WithTitle(request.Title)
                .WithDescription(request.Description)
                .WithInstructions(request.Instructions)
                .WithPreparationTime(request.PreparationTimeMinutes)
                .WithTotalWeightGrams(request.TotalWeightGrams)
                .WithMacros(
                    calories: request.Calories,
                    protein: request.Proteins,
                    carbohydrates: request.Carbohydrates,
                    fats: request.Fats)
                .CreatedAt(DateTime.UtcNow);

            var recipe = builder.Build();

            var added = await _recipeRepository.AddRecipeAsync(recipe);

            return new CreateRecipeResult { Success = true, RecipeId = added.Id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving generated recipe");
            return new CreateRecipeResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}


