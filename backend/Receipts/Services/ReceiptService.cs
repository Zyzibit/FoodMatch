using inzynierka.Products.Services;
using inzynierka.Receipts.Extensions;
using inzynierka.Receipts.Extensions.Builders;
using inzynierka.Receipts.Model;
using inzynierka.Receipts.Model.Recipe;
using inzynierka.Receipts.Repositories;
using inzynierka.Receipts.Requests;
using inzynierka.Receipts.Responses;
using inzynierka.Users.Services;

namespace inzynierka.Receipts.Services;


public class ReceiptService : IReceiptService
{
    private readonly IReceiptRepository _receiptRepository;
    private readonly ILogger<ReceiptService> _logger;
    private readonly IRecipeGeneratorService _recipeGeneratorService;
    private readonly IProductService _productService;
    private readonly IRecipeIngredientMatcher _ingredientMatcher;
    private readonly IUnitService _unitService;
    private readonly IUserService _userService;

    public ReceiptService(
        IReceiptRepository receiptRepository, 
        ILogger<ReceiptService> logger,
        IUserService userService,
        IRecipeGeneratorService recipeGeneratorService,
        IProductService productService,
        IRecipeIngredientMatcher ingredientMatcher,
        IUnitService unitService)
    {
        _receiptRepository = receiptRepository;
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

    public async Task<CreateReceiptResult> CreateReceiptAsync(string userId, CreateReceiptRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("CreateReceiptAsync called with empty or null userId");
                return new CreateReceiptResult 
                { 
                    Success = false, 
                    ErrorMessage = "User ID is required to create a receipt" 
                };
            }

            var userExists = await _userService.GetUserByIdAsync(userId);
            if (userExists == null)
            {
                _logger.LogError("User with ID {UserId} does not exist in the database", userId);
                return new CreateReceiptResult 
                { 
                    Success = false, 
                    ErrorMessage = "User not found in the database" 
                };
            }
            
            var ingredients = request.Ingredients.Select(i => new ReceiptIngredient
            {
                ProductId = i.ProductId,
                UnitId = i.UnitId,
                Quantity = i.Quantity,
                NormalizedQuantityInGrams = i.NormalizedQuantityInGrams
            }).ToList();

            var builder = ReceiptBuilder.Create()
                .ForUser(userId)
                .FromSource(ReceiptSource.User)
                .WithIngredients(ingredients)
                .WithAdditionalProducts(request.AdditionalProducts)
                .WithTitle(request.Title)
                .WithDescription(request.Description)
                .WithInstructions(request.Instructions)
                .WithServings(request.Servings)
                .WithPreparationTime(request.PreparationTimeMinutes)
                .WithTotalWeightGrams(request.TotalWeightGrams)
                .WithMacros(
                    calories: request.CaloriesPer100G,
                    protein: request.ProteinPer100G,
                    carbohydrates: request.CarbohydratesPer100G,
                    fats: request.FatsPer100G)
                .CreatedAt(DateTime.UtcNow);

            var receipt = builder.Build();

            var added = await _receiptRepository.AddReceiptAsync(receipt);
            return new CreateReceiptResult { Success = true, ReceiptId = added.Id };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating receipt");
            return new CreateReceiptResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public async Task<ReceiptDto?> GetReceiptAsync(int id)
    {
        var receipt = await _receiptRepository.GetReceiptByIdAsync(id);
        if (receipt == null) return null;

        return receipt.ToDto();
    }

    public async Task<ReceiptsListResult> GetAllReceiptsAsync(int limit = 50, int offset = 0)
    {
        var (receipts, total) = await _receiptRepository.GetAllReceiptsAsync(limit, offset);
        var dtoList = receipts.ToDtoList().ToList();
        return new ReceiptsListResult { Success = true, Receipts = dtoList, TotalCount = total };
    }

    public async Task<ReceiptsListResult> GetUserReceiptsAsync(string userId, int limit = 50, int offset = 0)
    {
        var (receipts, total) = await _receiptRepository.GetUserReceiptsAsync(userId, limit, offset);
        var dtoList = receipts.ToDtoList().ToList();
        return new ReceiptsListResult { Success = true, Receipts = dtoList, TotalCount = total };
    }

    public async Task<CreateReceiptResult> GenerateRecipeWithAiAsync(string userId, GenerateRecipeWithAiRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(userId))
            {
                _logger.LogError("GenerateRecipeWithAiAsync called with empty or null userId");
                return new CreateReceiptResult 
                { 
                    Success = false, 
                    ErrorMessage = "User ID is required to generate a recipe" 
                };
            }

            var userExists = await _userService.GetUserByIdAsync(userId);
            if (userExists == null)
            {
                _logger.LogError("User with ID {UserId} does not exist in the database", userId);
                return new CreateReceiptResult 
                { 
                    Success = false, 
                    ErrorMessage = "User not found in the database" 
                };
            }
            var userPreferences = await _userService.GetUserFoodPreferencesAsync(userId);
            
            var preferences = request.Preferences ?? CreatePreferencesFromUser(userPreferences);
            
            if (!string.IsNullOrEmpty(request.MealType) && preferences != null && userPreferences != null)
            {
                preferences.MealType = request.MealType;
                
                // Obliczanie celów żywieniowych dla tego posiłku na podstawie preferencji użytkownika
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
                        "Calculated nutritional goals for {MealType}: {Calories} kcal, {Protein}g protein, {Carbs}g carbs, {Fat}g fat", 
                        request.MealType, mealGoals.Calories, mealGoals.Protein, mealGoals.Carbohydrates, mealGoals.Fat);
                }
                
                // Przekazujemy również dzienne cele do AI
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
            
            // if (!productsList.Any())
            // {
            //     return new CreateReceiptResult 
            //     { 
            //         Success = false, 
            //         ErrorMessage = "No products found with the provided IDs" 
            //     };
            // }
            //
            var foundProductIds = productsList
                .Select(p => int.TryParse(p.Id, out var id) ? id : -1)
                .Where(id => id != -1)
                .ToList();
            
            var missingProductIds = request.ProductIds.Except(foundProductIds).ToList();
                    
            if (missingProductIds.Any())
            {
                _logger.LogWarning("Missing products with IDs: {MissingIds}", string.Join(", ", missingProductIds));
            }
            
            var ingredientNames = productsList
                .Select(p => string.IsNullOrWhiteSpace(p.Name) 
                    ? (string.IsNullOrWhiteSpace(p.Brand) ? $"Product {p.Id}" : p.Brand)
                    : p.Name)
                .ToList();
            
            var aiRequest = new GenerateRecipeRequest
            {
                AvailableIngredients = ingredientNames,
                Preferences = preferences,
                CuisineType = request.CuisineType,
                DesiredServings = request.DesiredServings,
                MaxPreparationTimeMinutes = request.MaxPreparationTimeMinutes,
                AdditionalInstructions = request.AdditionalInstructions
            };

            var aiResult = await _recipeGeneratorService.GenerateRecipeAsync(aiRequest);

            if (!aiResult.Success || aiResult.Recipe == null)
            {
                return new CreateReceiptResult 
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

            var ingredients = new List<ReceiptIngredient>();
            var additionalProductsList = new List<string>();

            foreach (var product in usedProducts)
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
                    
                    var normalizedQuantityInGrams = aiIngredient.NormalizedQuantityInGrams;
                    
                    ingredients.Add(new ReceiptIngredient
                    {
                        ProductId = productId,
                        UnitId = unitId,
                        NormalizedQuantityInGrams = normalizedQuantityInGrams,
                        Quantity = quantity.Value
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add ingredient {ProductName} with unit {Unit}", 
                        productName, aiIngredient.Unit);
                }
            }

            foreach (var additionalIngredient in additionalIngredientsData)
            {
                try
                {
                    var aiGeneratedProduct = await _productService.CreateAiGeneratedProductAsync(additionalIngredient);
                    var unitId = await GetUnitIdForIngredientAsync(additionalIngredient.Unit);
                    
                    ingredients.Add(new ReceiptIngredient
                    {
                        ProductId = aiGeneratedProduct.Id,
                        UnitId = unitId,
                        NormalizedQuantityInGrams = additionalIngredient.NormalizedQuantityInGrams,
                        Quantity = additionalIngredient.Quantity
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to add AI-generated ingredient {IngredientName}", additionalIngredient.Name);
                    additionalProductsList.Add($"{additionalIngredient.Name} ({additionalIngredient.Quantity} {additionalIngredient.Unit})");
                }
            }

            var builder = ReceiptBuilder.Create()
                .ForUser(userId)
                .FromSource(ReceiptSource.AI)
                .WithTitle(generatedRecipe.Title)
                .WithDescription(generatedRecipe.Description)
                .WithInstructions(generatedRecipe.Instructions)
                .WithServings(generatedRecipe.Servings)
                .WithPreparationTime(generatedRecipe.PreparationTimeMinutes)
                .WithTotalWeightGrams(generatedRecipe.TotalWeightGrams)
                .WithMacros(
                    calories: generatedRecipe.EstimatedCalories,
                    protein: generatedRecipe.EstimatedProtein,
                    carbohydrates: generatedRecipe.EstimatedCarbohydrates,
                    fats: generatedRecipe.EstimatedFats)
                .WithIngredients(ingredients)
                .WithAdditionalProducts(additionalProductsList)
                .CreatedAt(DateTime.UtcNow);

            var receipt = builder.Build();

            var added = await _receiptRepository.AddReceiptAsync(receipt);
            
            
            _logger.LogInformation("AI-generated recipe created with ID: {ReceiptId}, with {Count} ingredients and {AdditionalCount} AI-generated products", 
                added.Id, ingredients.Count, additionalIngredientsData.Count);
            
            return new CreateReceiptResult 
            { 
                Success = true, 
                ReceiptId = added.Id 
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recipe with AI");
            return new CreateReceiptResult 
            { 
                Success = false, 
                ErrorMessage = ex.Message 
            };
        }
    }
    
    private DietaryPreferences? CreatePreferencesFromUser(Users.Responses.FoodPreferencesDto? userPreferences)
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
            MaxCalories = userPreferences.DailyCalorieGoal,
            
            DailyCalorieGoal = userPreferences.DailyCalorieGoal,
            DailyProteinGoal = userPreferences.DailyProteinGoal,
            DailyCarbohydrateGoal = userPreferences.DailyCarbohydrateGoal,
            DailyFatGoal = userPreferences.DailyFatGoal
        };
    }
    
    private MealNutritionalGoals? CalculateMealNutritionalGoals(string mealType, Users.Responses.FoodPreferencesDto userPreferences)
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
