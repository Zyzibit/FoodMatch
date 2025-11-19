using inzynierka.Products.Mappings;
using inzynierka.Products.Services;
using inzynierka.Recipes.Builders;
using inzynierka.Recipes.Extensions;
using inzynierka.Recipes.Model;
using inzynierka.Recipes.Repositories;
using inzynierka.Recipes.Requests;
using inzynierka.Recipes.Responses;
using inzynierka.Users.Services;

namespace inzynierka.Recipes.Services;


public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ILogger<RecipeService> _logger;
    private readonly IRecipePreferenceMapper _preferenceMapper;
    private readonly IRecipeGeneratorService _recipeGeneratorService;
    private readonly IProductService _productService;
    private readonly IProductMapper _productMapper;
    private readonly IRecipeIngredientMatcher _ingredientMatcher;
    private readonly IRecipeIngredientProcessor _ingredientProcessor;
    private readonly IUserService _userService;

    public RecipeService(
        IRecipeRepository recipeRepository, 
        ILogger<RecipeService> logger,
        IUserService userService,
        IRecipeGeneratorService recipeGeneratorService,
        IProductService productService,
        IProductMapper productMapper,
        IRecipePreferenceMapper preferenceMapper,
        IRecipeIngredientProcessor ingredientProcessor, 
        IRecipeIngredientMatcher ingredientMatcher)
    {
        _recipeRepository = recipeRepository;
        _logger = logger;
        _userService = userService;
        _recipeGeneratorService = recipeGeneratorService;
        _productService = productService;
        _productMapper = productMapper;
        _preferenceMapper = preferenceMapper;
        _ingredientProcessor = ingredientProcessor;
        _ingredientMatcher = ingredientMatcher;
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

            var added = await _recipeRepository.AddRecipeAsync(recipe);
            return new CreateRecipeResult { Success = true, RecipeId = added.Id };
        }
                catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating recipe");
            return new CreateRecipeResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<RecipeDto?> GetRecipeAsync(int id)
    {
        var recipe = await _recipeRepository.GetRecipeByIdAsync(id);
        if (recipe == null) return null;

        return recipe.ToDto(_productMapper);
    }

    public async Task<RecipeListResult> Recipes(int limit = 50, int offset = 0)
    {
        var (recipes, total) = await _recipeRepository.GetAllRecipesAsync(limit, offset);
        var dtoList = recipes.ToDtoList(_productMapper).ToList();
        return new RecipeListResult { Success = true, Recipes = dtoList, TotalCount = total };
    }

    public async Task<RecipeListResult> GetUserRecipesAsync(string userId, int limit = 50, int offset = 0)
    {
        var (recipes, total) = await _recipeRepository.GetUserRecipesAsync(userId, limit, offset);
        var dtoList = recipes.ToDtoList(_productMapper).ToList();
        return new RecipeListResult { Success = true, Recipes = dtoList, TotalCount = total };
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
            
            var userPreferences = await _userService.GetUserFoodPreferencesAsync(userId);
            
            var preferences = request.Preferences ?? _preferenceMapper.MapFromUserPreferences(userPreferences);
            
            if (!string.IsNullOrEmpty(request.MealType) && preferences != null && userPreferences != null)
            {
                _preferenceMapper.ApplyMealTypeGoals(preferences, request.MealType, userPreferences);
            }
            
            var productsInfo = await _productService.GetProductsByIdsAsync(request.ProductIds);
            var productsList = productsInfo.ToList();
            
            var foundProductIds = productsList
                .Select(p => int.TryParse(p.Id, out var id) ? id : -1)
                .Where(id => id != -1)
                .ToList();
            
            var missingProductIds = request.ProductIds.Except(foundProductIds).ToList();
                    
            if (missingProductIds.Any())
            {
                _logger.LogWarning("Missing products with IDs: {MissingIds}", string.Join(", ", missingProductIds));
            }
            
            var ingredientNames = _ingredientMatcher.PrepareIngredientNames(productsList, request.AvailableIngredients);
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

            var userProvidedIngredientNames = ingredientNames.Select(n => n.ToLowerInvariant()).ToList();
            var additionalIngredientsData = _ingredientMatcher.GetAdditionalIngredients(
                userProvidedIngredientNames,
                generatedRecipe.Ingredients);

            var usedProducts = _ingredientMatcher.GetMatchingProducts(productsList, generatedRecipe.Ingredients);

            var previewIngredients = await _ingredientProcessor.ProcessUserProvidedIngredientsAsync(
                usedProducts,
                generatedRecipe.Ingredients);

            var aiGeneratedIngredients = await _ingredientProcessor.ProcessAiGeneratedIngredientsAsync(
                additionalIngredientsData);
            
            previewIngredients.AddRange(aiGeneratedIngredients);

            var additionalProductsList = additionalIngredientsData
                .Where(ai => !aiGeneratedIngredients.Any(agi => agi.ProductName == ai.Name))
                .Select(ai => $"{ai.Name} ({ai.Quantity} {ai.Unit})")
                .ToList();


            var preview = new GeneratedRecipePreviewDto
            {
                Title = generatedRecipe.Title,
                Description = generatedRecipe.Description,
                Instructions = generatedRecipe.Instructions,
                PreparationTimeMinutes = generatedRecipe.PreparationTimeMinutes,
                TotalWeightGrams = generatedRecipe.TotalWeightGrams,
                Calories = generatedRecipe.EstimatedCalories,
                Proteins = generatedRecipe.EstimatedProtein,
                Carbohydrates = generatedRecipe.EstimatedCarbohydrates,
                Fats = generatedRecipe.EstimatedFats,
                Ingredients = previewIngredients,
                AdditionalProducts = additionalProductsList
            };


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


