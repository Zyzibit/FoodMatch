using inzynierka.Products.Model;
using inzynierka.Products.Services;
using inzynierka.Recipes.Builders;
using inzynierka.Recipes.Extensions;
using inzynierka.Recipes.Model;
using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.Recipes.Repositories;
using inzynierka.Recipes.Requests;
using inzynierka.Recipes.Responses;
using inzynierka.Units.Services;
using inzynierka.Users.Responses;
using inzynierka.Users.Services;

namespace inzynierka.Recipes.Services;


public class RecipeService : IRecipeService
{
    private readonly IRecipeRepository _recipeRepository;
    private readonly ILogger<RecipeService> _logger;
    private readonly IRecipeGeneratorService _recipeGeneratorService;
    private readonly IProductService _productService;
    private readonly IRecipeIngredientMatcher _ingredientMatcher;
    private readonly IUnitService _unitService;
    private readonly IUserService _userService;

    public RecipeService(
        IRecipeRepository recipeRepository, 
        ILogger<RecipeService> logger,
        IUserService userService,
        IRecipeGeneratorService recipeGeneratorService,
        IProductService productService,
        IRecipeIngredientMatcher ingredientMatcher,
        IUnitService unitService)
    {
        _recipeRepository = recipeRepository;
        _logger = logger;
        _userService = userService;
        _recipeGeneratorService = recipeGeneratorService;
        _productService = productService;
        _ingredientMatcher = ingredientMatcher;
        _unitService = unitService;
    }
    
    public async Task<int> GetUnitIdForIngredientAsync(string? unitName)
    {
        if (string.IsNullOrEmpty(unitName))
        {
            throw new ArgumentException("Unit name cannot be null or empty", nameof(unitName));
        }
            
        try
        {
            var units = await _unitService.GetAllUnitsAsync();
            
            if (!units.Any())
            {
                throw new InvalidOperationException("No units found in the database. Please ensure units are seeded.");
            }
            
            var unit = units.FirstOrDefault(u => 
                u.Name.Equals(unitName.Trim(), StringComparison.OrdinalIgnoreCase));
            
            if (unit != null)
            {
                return unit.UnitId;
            }
            
            _logger.LogWarning("Unit '{UnitName}' not found, using default 'g'. Available units: {Units}", 
                unitName, string.Join(", ", units.Select(u => u.Name)));
            
            var defaultUnit = units.FirstOrDefault(u => 
                u.Name.Equals("gram", StringComparison.OrdinalIgnoreCase));
            
            if (defaultUnit != null)
            {
                return defaultUnit.UnitId;
            }
            
            throw new InvalidOperationException($"Unit '{unitName}' not found and default unit 'gram' is missing. Available: {string.Join(", ", units.Select(u => u.Name))}");
        }
        catch (Exception ex) when (ex is not InvalidOperationException && ex is not ArgumentException)
        {
            _logger.LogError(ex, "Error getting unit ID for unit name: {UnitName}", unitName);
            throw new InvalidOperationException($"Failed to retrieve unit '{unitName}' from database", ex);
        }
    }
    
    public decimal? GetQuantityForIngredient(string? productName, List<GeneratedRecipeIngredient> aiIngredients)
    {
        if (string.IsNullOrEmpty(productName))
        {
            _logger.LogWarning("Product name is null or empty, cannot determine quantity");
            return null;
        }

        var matchingIngredient = aiIngredients
            .FirstOrDefault(ai => productName.Contains(ai.Name, StringComparison.OrdinalIgnoreCase) ||
                                 ai.Name.Contains(productName, StringComparison.OrdinalIgnoreCase));
        
        if (matchingIngredient == null)
        {
            _logger.LogWarning("No matching AI ingredient found for product: {ProductName}", productName);
            return null;
        }

        return matchingIngredient.Quantity;
    }

    public async Task<CreateRecipeResult> CreateRecipeAsync(string userId, CreateRecipeRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("CreateRecipeAsync called with empty or null userId");
                return new CreateRecipeResult 
                { 
                    Success = false, 
                    ErrorMessage = "User ID is required to create a recipe" 
                };
            }

            var userExists = await _userService.GetUserByIdAsync(userId);
            if (userExists == null)
            {
                _logger.LogError("User with ID {UserId} does not exist in the database", userId);
                return new CreateRecipeResult 
                { 
                    Success = false, 
                    ErrorMessage = "User not found in the database" 
                };
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

    public async Task<GenerateRecipePreviewResult> GenerateRecipePreviewAsync(string userId, GenerateRecipeRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("GenerateRecipePreviewAsync called with empty or null userId");
                return new GenerateRecipePreviewResult 
                { 
                    Success = false, 
                    ErrorMessage = "User ID is required to generate a recipe" 
                };
            }

            var userExists = await _userService.GetUserByIdAsync(userId);
            if (userExists == null)
            {
                _logger.LogError("User with ID {UserId} does not exist in the database", userId);
                return new GenerateRecipePreviewResult 
                { 
                    Success = false, 
                    ErrorMessage = "User not found in the database" 
                };
            }
            
            var userPreferences = await _userService.GetUserFoodPreferencesAsync(userId);
            
            var preferences = request.Preferences ?? CreatePreferencesFromUser(userPreferences);
            
            _logger.LogInformation("MealType from request: '{MealType}', Preferences: {HasPreferences}, UserPreferences: {HasUserPreferences}", 
                request.MealType ?? "NULL", preferences != null, userPreferences != null);
            
            if (!string.IsNullOrEmpty(request.MealType) && preferences != null && userPreferences != null)
            {
                preferences.MealType = request.MealType;
                
                var mealGoals = CalculateMealNutritionalGoals(request.MealType, userPreferences);
                
                if (mealGoals != null)
                {
                    if (!preferences.TargetMealCalories.HasValue && mealGoals.Calories.HasValue)
                    {
                        preferences.TargetMealCalories = mealGoals.Calories.Value;
                    }
                    
                    if (!preferences.TargetMealProtein.HasValue && mealGoals.Protein.HasValue)
                    {
                        preferences.TargetMealProtein = mealGoals.Protein.Value;
                    }
                    
                    if (!preferences.TargetMealCarbohydrates.HasValue && mealGoals.Carbohydrates.HasValue)
                    {
                        preferences.TargetMealCarbohydrates = mealGoals.Carbohydrates.Value;
                    }
                    
                    if (!preferences.TargetMealFat.HasValue && mealGoals.Fat.HasValue)
                    {
                        preferences.TargetMealFat = mealGoals.Fat.Value;
                    }
                    
                    _logger.LogInformation(
                        "Calculated nutritional goals for {MealType}: {Calories} kcal, {Proteins}g protein, {Carbs}g carbs, {Fat}g fat", 
                        request.MealType, mealGoals.Calories, mealGoals.Protein, mealGoals.Carbohydrates, mealGoals.Fat);
                }
                
                if (!preferences.DailyCalorieGoal.HasValue)
                {
                    preferences.DailyCalorieGoal = userPreferences.DailyCalorieGoal ?? userPreferences.CalculatedDailyCalories;
                }
                
                if (!preferences.DailyProteinGoal.HasValue)
                {
                    preferences.DailyProteinGoal = userPreferences.DailyProteinGoal;
                }
                
                if (!preferences.DailyCarbohydrateGoal.HasValue)
                {
                    preferences.DailyCarbohydrateGoal = userPreferences.DailyCarbohydrateGoal;
                }
                
                if (!preferences.DailyFatGoal.HasValue)
                {
                    preferences.DailyFatGoal = userPreferences.DailyFatGoal;
                }
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
            
            var ingredientNames = new List<string>();
            
            if (productsList.Any())
            {
                ingredientNames.AddRange(productsList
                    .Select(p => string.IsNullOrWhiteSpace(p.Name) 
                        ? (string.IsNullOrWhiteSpace(p.Brand) ? $"Product {p.Id}" : p.Brand)
                        : p.Name));
            }
            
            if (request.AvailableIngredients.Any())
            {
                ingredientNames.AddRange(request.AvailableIngredients);
            }
            
            request.AvailableIngredients = ingredientNames;
            request.Preferences = preferences;

            if (preferences != null)
            {
                _logger.LogInformation(
                    "Sending to AI - MealType: {MealType}, TargetMealCalories: {Calories}, TargetMealProtein: {Proteins}, TargetMealCarbs: {Carbs}, TargetMealFat: {Fat}",
                    preferences.MealType ?? "NULL",
                    preferences.TargetMealCalories?.ToString() ?? "NULL",
                    preferences.TargetMealProtein?.ToString() ?? "NULL",
                    preferences.TargetMealCarbohydrates?.ToString() ?? "NULL",
                    preferences.TargetMealFat?.ToString() ?? "NULL");
            }

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

            var usedProducts = _ingredientMatcher.GetMatchingProducts(
                productsList,
                generatedRecipe.Ingredients);

            if (usedProducts.Count < productsList.Count)
            {
                var unusedProducts = productsList.Except(usedProducts)
                    .Select(p => string.IsNullOrWhiteSpace(p.Name) ? p.Id : p.Name);
                _logger.LogInformation("AI used {UsedCount}/{TotalCount} products. Unused: {UnusedProducts}", 
                    usedProducts.Count, productsList.Count, string.Join(", ", unusedProducts));
            }

            var previewIngredients = new List<PreviewRecipeIngredientDto>();
            var additionalProductsList = new List<string>();

            var distinctProducts = usedProducts
                .GroupBy(p => p.Id)
                .Select(g => g.First())
                .ToList();

            if (distinctProducts.Count < usedProducts.Count)
            {
                _logger.LogWarning("Removed {DuplicateCount} duplicate products from the list", 
                    usedProducts.Count - distinctProducts.Count);
            }

            // Process user-provided products
            foreach (var product in distinctProducts)
            {
                var aiIngredient = _ingredientMatcher.FindMatchingRecipeIngredient(product, generatedRecipe.Ingredients);

                if (aiIngredient == null)
                {
                    _logger.LogWarning("No matching AI ingredient found for product: {ProductName}", 
                        _ingredientMatcher.GetProductDisplayName(product));
                    continue;
                }

                var productName = _ingredientMatcher.GetProductDisplayName(product);
                var quantity = GetQuantityForIngredient(productName, generatedRecipe.Ingredients);

                if (!quantity.HasValue)
                {
                    _logger.LogWarning("Could not determine quantity for product: {ProductName}", productName);
                    continue;
                }

                try
                {
                    if (!int.TryParse(product.Id, out var productId))
                    {
                        _logger.LogError("Failed to parse product ID: {ProductId}", product.Id);
                        continue;
                    }

                    var unitId = await GetUnitIdForIngredientAsync(aiIngredient.Unit);
                    var units = await _unitService.GetAllUnitsAsync();
                    var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.Name ?? aiIngredient.Unit;
                    
                    var normalizedQuantityInGrams = aiIngredient.NormalizedQuantityInGrams ?? 0;
                    
                    // Calculate nutritional values for this ingredient
                    var calories = (decimal)(product.Nutrition?.Calories ?? 0) * (normalizedQuantityInGrams / 100m);
                    var protein = (decimal)(product.Nutrition?.Proteins ?? 0) * (normalizedQuantityInGrams / 100m);
                    var carbohydrates = (decimal)(product.Nutrition?.Carbohydrates ?? 0) * (normalizedQuantityInGrams / 100m);
                    var fats = (decimal)(product.Nutrition?.Fat ?? 0) * (normalizedQuantityInGrams / 100m);
                    
                    previewIngredients.Add(new PreviewRecipeIngredientDto
                    {
                        ProductId = productId,
                        ProductName = productName,
                        UnitId = unitId,
                        UnitName = unitName,
                        Quantity = quantity.Value,
                        NormalizedQuantityInGrams = normalizedQuantityInGrams,
                        Source = ProductSource.User,
                        Calories = calories,
                        Protein = protein,
                        Carbohydrates = carbohydrates,
                        Fats = fats
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process ingredient {ProductName} with unit {Unit}", 
                        productName, aiIngredient.Unit);
                }
            }

            // Process AI-generated ingredients
            foreach (var additionalIngredient in additionalIngredientsData)
            {
                try
                {
                    var aiGeneratedProduct = await _productService.CreateAiGeneratedProductAsync(additionalIngredient);
                    var unitId = await GetUnitIdForIngredientAsync(additionalIngredient.Unit);
                    var units = await _unitService.GetAllUnitsAsync();
                    var unitName = units.FirstOrDefault(u => u.UnitId == unitId)?.Name ?? additionalIngredient.Unit;
                    
                    // Get nutritional values from AI-generated product
                    var normalizedQuantityInGrams = additionalIngredient.NormalizedQuantityInGrams ?? 0;
                    var productDto = await _productService.GetProductAsync(aiGeneratedProduct.Id.ToString());
                    
                    decimal calories = 0, protein = 0, carbohydrates = 0, fats = 0;
                    
                    if (productDto.Success && productDto.Product != null && productDto.Product.Nutrition != null)
                    {
                        calories = (decimal)(productDto.Product.Nutrition.Calories ?? 0) * (normalizedQuantityInGrams / 100m);
                        protein = (decimal)(productDto.Product.Nutrition.Proteins ?? 0) * (normalizedQuantityInGrams / 100m);
                        carbohydrates = (decimal)(productDto.Product.Nutrition.Carbohydrates ?? 0) * (normalizedQuantityInGrams / 100m);
                        fats = (decimal)(productDto.Product.Nutrition.Fat ?? 0) * (normalizedQuantityInGrams / 100m);
                    }
                    
                    previewIngredients.Add(new PreviewRecipeIngredientDto
                    {
                        ProductId = aiGeneratedProduct.Id,
                        ProductName = additionalIngredient.Name,
                        UnitId = unitId,
                        UnitName = unitName,
                        Quantity = additionalIngredient.Quantity,
                        NormalizedQuantityInGrams = normalizedQuantityInGrams,
                        Source = ProductSource.AI,
                        Calories = calories,
                        Protein = protein,
                        Carbohydrates = carbohydrates,
                        Fats = fats
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process AI-generated ingredient {IngredientName}", additionalIngredient.Name);
                    additionalProductsList.Add($"{additionalIngredient.Name} ({additionalIngredient.Quantity} {additionalIngredient.Unit})");
                }
            }

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

            _logger.LogInformation("Generated recipe preview with {Count} ingredients and {AdditionalCount} additional products", 
                previewIngredients.Count, additionalProductsList.Count);

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
                _logger.LogError("SaveGeneratedRecipeAsync called with empty or null userId");
                return new CreateRecipeResult 
                { 
                    Success = false, 
                    ErrorMessage = "User ID is required to save a recipe" 
                };
            }

            var userExists = await _userService.GetUserByIdAsync(userId);
            if (userExists == null)
            {
                _logger.LogError("User with ID {UserId} does not exist in the database", userId);
                return new CreateRecipeResult 
                { 
                    Success = false, 
                    ErrorMessage = "User not found in the database" 
                };
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
            
            _logger.LogInformation("Saved AI-generated recipe with ID: {RecipeId} for user {UserId}", 
                added.Id, userId);
            
            return new CreateRecipeResult { Success = true, RecipeId = added.Id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving generated recipe");
            return new CreateRecipeResult { Success = false, ErrorMessage = ex.Message };
        }
    }
    
    private DietaryPreferences? CreatePreferencesFromUser(FoodPreferencesDto? userPreferences)
    {
        if (userPreferences == null)
        {
            return null;
        }
        
        return new DietaryPreferences
        {
            IsVegan = userPreferences.IsVegan ?? false,
            IsVegetarian = userPreferences.IsVegetarian ?? false,
            IsGlutenFree = userPreferences.HasGlutenIntolerance ?? false,
            IsLactoseFree = userPreferences.HasLactoseIntolerance ?? false,
            Allergies = userPreferences.Allergies ?? new List<string>(),
            DislikedIngredients = new List<string>(),
            
            DailyCalorieGoal = userPreferences.DailyCalorieGoal,
            DailyProteinGoal = userPreferences.DailyProteinGoal,
            DailyCarbohydrateGoal = userPreferences.DailyCarbohydrateGoal,
            DailyFatGoal = userPreferences.DailyFatGoal
        };
    }
    
    private MealNutritionalGoals? CalculateMealNutritionalGoals(string mealType, FoodPreferencesDto userPreferences)
    {
        var mealTypeLower = mealType.ToLowerInvariant();
        
        if (mealTypeLower == "breakfast")
        {
            return new MealNutritionalGoals
            {
                Calories = userPreferences.BreakfastCalories,
                Protein = userPreferences.BreakfastProteinGoal,
                Carbohydrates = userPreferences.BreakfastCarbohydrateGoal,
                Fat = userPreferences.BreakfastFatGoal
            };
        }
        else if (mealTypeLower == "lunch")
        {
            return new MealNutritionalGoals
            {
                Calories = userPreferences.LunchCalories,
                Protein = userPreferences.LunchProteinGoal,
                Carbohydrates = userPreferences.LunchCarbohydrateGoal,
                Fat = userPreferences.LunchFatGoal
            };
        }
        else if (mealTypeLower == "dinner")
        {
            return new MealNutritionalGoals
            {
                Calories = userPreferences.DinnerCalories,
                Protein = userPreferences.DinnerProteinGoal,
                Carbohydrates = userPreferences.DinnerCarbohydrateGoal,
                Fat = userPreferences.DinnerFatGoal
            };
        }
        else if (mealTypeLower == "snack")
        {
            return new MealNutritionalGoals
            {
                Calories = userPreferences.SnackCalories,
                Protein = userPreferences.SnackProteinGoal,
                Carbohydrates = userPreferences.SnackCarbohydrateGoal,
                Fat = userPreferences.SnackFatGoal
            };
        }
        
        _logger.LogWarning("Unknown meal type: {MealType}. Cannot calculate nutritional goals.", mealType);
        return null;
    }
}

internal class MealNutritionalGoals
{
    public int? Calories { get; set; }
    public int? Protein { get; set; }
    public int? Carbohydrates { get; set; }
    public int? Fat { get; set; }
}
