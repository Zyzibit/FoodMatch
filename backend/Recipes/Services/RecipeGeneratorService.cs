using System.Text.Json;
using inzynierka.AI.OpenAI;
using inzynierka.AI.OpenAI.Services;
using inzynierka.Products.Model;
using inzynierka.Products.Repositories;
using inzynierka.Recipes.Model;
using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.Recipes.Requests;
using inzynierka.Recipes.Responses;
using inzynierka.Units.Services;

namespace inzynierka.Recipes.Services;

    public class RecipeGeneratorService(
        IAiClient aiClient,
        ILogger<RecipeGeneratorService> logger,
        IPromptConfigService promptConfigService,
        IUnitService unitService,
        IProductRepository productRepository,
        IConfiguration configuration)
        : IRecipeGeneratorService
    {
        private readonly IAiClient _aiClient = aiClient;
        private readonly ILogger<RecipeGeneratorService> _logger = logger;
        private readonly IPromptConfigService _promptConfigService = promptConfigService;
        private readonly IUnitService _unitService = unitService;
        private readonly IProductRepository _productRepository = productRepository;
        private readonly string _configPath = configuration["Recipes:PromptConfigPath"] ?? 
                                              Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recipes", "Config", "prompt_config.json");

        public async Task<GenerateRecipeResponse> GenerateRecipeAsync(GenerateRecipeRequest request)
        {
            
            try
            {
                var config = await _promptConfigService.LoadConfigAsync(_configPath);

                var products = new List<Product>();
                if (request.ProductIds.Any())
                {
                    var productsEnumerable = await _productRepository.GetProductsByIdsAsync(request.ProductIds);
                    products = productsEnumerable.ToList();
                    _logger.LogDebug("Pobrano {Count} produktów z bazy danych", products.Count);
                }
                
                var data = await PreparePromptDataAsync(request, products);
                var userPrompt = _promptConfigService.RenderPrompt(config, data);
                
                _logger.LogDebug("Generated user prompt: {UserPrompt}", userPrompt);
                
                var result = await _aiClient.SendPromptForJsonAsync(config.SystemMessage, userPrompt);
                
                if (result == null)
                {
                    return new GenerateRecipeResponse
                    {
                        Success = false,
                        ErrorMessage = "Unable to get a valid response from AI service."
                    };
                }
                
                var recipe = ParseRecipeFromJson(result.Value, products);
                
                return new GenerateRecipeResponse
                {
                    Success = true,
                    Recipe = recipe
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error in recipe generation");
                return new GenerateRecipeResponse
                {
                    Success = false,
                    ErrorMessage = "Error while parsing AI response: " + ex.Message
                };
            }
            catch (Exception ex)
            {
                return new GenerateRecipeResponse
                {
                    Success = false,
                    ErrorMessage = $"Unexpected error while generating recipe: {ex.Message}"
                };
            }
        }

        private GeneratedRecipe ParseRecipeFromJson(JsonElement jsonElement, List<Product> products)
        {
            try
            {
                if (!jsonElement.TryGetProperty("title", out _))
                {
                    _logger.LogError("Missing required property 'title' in AI response");
                    throw new JsonException("AI response missing 'title' property");
                }
                
                if (!jsonElement.TryGetProperty("ingredients", out _))
                {
                    _logger.LogError("Missing required property 'ingredients' in AI response");
                    throw new JsonException("AI response missing 'ingredients' property");
                }
                
                var recipe = new GeneratedRecipe
                {
                    Title = jsonElement.GetProperty("title").GetString() ?? "Invalid title",
                    Description = jsonElement.TryGetProperty("description", out var desc) ? desc.GetString() ?? "" : "",
                    Instructions = jsonElement.TryGetProperty("instructions", out var instr) ? instr.GetString() ?? "" : "",
                    PreparationTimeMinutes = jsonElement.TryGetProperty("preparationTimeMinutes", out var prep) ? prep.GetInt32() : 0
                };

                var ingredientsArray = jsonElement.GetProperty("ingredients");
                foreach (var ingredientElement in ingredientsArray.EnumerateArray())
                {
                    var ingredientName = ingredientElement.GetProperty("name").GetString() ?? "";
                    var normalizedQuantity = GetDecimalProperty(ingredientElement, "normalizedQuantityInGrams");
                    
                    var matchedProduct = FindMatchingProduct(ingredientName, products);
                    
                    recipe.Ingredients.Add(new GeneratedRecipeIngredient
                    {
                        Name = ingredientName,
                        Quantity = GetDecimalProperty(ingredientElement, "quantity"),
                        Unit = ingredientElement.GetProperty("unit").GetString() ?? "",
                        NormalizedQuantityInGrams = normalizedQuantity > 0 ? normalizedQuantity : null,
                        EstimatedCalories = GetDecimalProperty(ingredientElement, "estimatedCalories"),
                        EstimatedProteins = GetDecimalProperty(ingredientElement, "estimatedProteins"),
                        EstimatedCarbohydrates = GetDecimalProperty(ingredientElement, "estimatedCarbohydrates"),
                        EstimatedFats = GetDecimalProperty(ingredientElement, "estimatedFats"),
                        ProductId = matchedProduct?.Id
                    });
                }

                decimal actualTotalWeight = 0;
                foreach (var i in recipe.Ingredients)
                {
                    if (i.NormalizedQuantityInGrams.HasValue) actualTotalWeight += i.NormalizedQuantityInGrams.Value;
                }

                var actualTotalCalories = recipe.Ingredients.Sum(i => i.EstimatedCalories);
                var actualTotalProtein = recipe.Ingredients.Sum(i => i.EstimatedProteins);
                var actualTotalCarbs = recipe.Ingredients.Sum(i => i.EstimatedCarbohydrates);
                var actualTotalFats = recipe.Ingredients.Sum(i => i.EstimatedFats);
                
                var aiTotalWeight = GetIntProperty(jsonElement, "totalWeightGrams");
                // if (aiTotalWeight > 0 && Math.Abs(aiTotalWeight - actualTotalWeight) > 1m)
                // {
                //     _logger.LogWarning(
                //         "AI provided totalWeightGrams ({AIWeight}g) differs from actual sum ({ActualWeight}g). Using AI provided weight.",
                //         aiTotalWeight, actualTotalWeight);
                // }
                
                // if (aiTotalCalories > 0 && Math.Abs(aiTotalCalories - actualTotalCalories) > 10m)
                // {
                //     _logger.LogWarning(
                //         "AI provided estimatedCalories ({AICalories} kcal) differs from actual sum ({ActualCalories} kcal). Using actual values.",
                //         aiTotalCalories, actualTotalCalories);
                // }
                _logger.LogWarning("AI total weight: {AIWeight}g, Actual total weight: {ActualWeight}g", aiTotalWeight, actualTotalWeight);
                recipe.TotalWeightGrams = aiTotalWeight;
                recipe.EstimatedCalories = actualTotalCalories;
                recipe.EstimatedProtein = actualTotalProtein;
                recipe.EstimatedCarbohydrates = actualTotalCarbs;
                recipe.EstimatedFats = actualTotalFats;

                return recipe;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error parsing recipe JSON structure");
                throw new JsonException("Błąd parsowania struktury przepisu", ex);
            }
        }

        private int GetIntProperty(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return 0;

            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetInt32(),
                JsonValueKind.String when int.TryParse(property.GetString(), out var value) => value,
                _ => 0
            };
        }

        private decimal GetDecimalProperty(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var property))
                return 0;

            return property.ValueKind switch
            {
                JsonValueKind.Number => property.GetDecimal(),
                JsonValueKind.String when decimal.TryParse(property.GetString(), out var value) => value,
                _ => 0
            };
        }
        
        private Product? FindMatchingProduct(string ingredientName, List<Product> products)
        {
            if (string.IsNullOrWhiteSpace(ingredientName) || !products.Any())
                return null;

            var normalizedIngredientName = ingredientName.ToLowerInvariant().Trim();
            
            var exactMatch = products.FirstOrDefault(p => 
                p.ProductName?.ToLowerInvariant().Trim() == normalizedIngredientName);
            if (exactMatch != null)
            {
                _logger.LogDebug("Dokładne dopasowanie: '{IngredientName}' -> Product ID {ProductId}", ingredientName, exactMatch.Id);
                return exactMatch;
            }
            
            var partialMatch = products.FirstOrDefault(p => 
                !string.IsNullOrWhiteSpace(p.ProductName) && 
                normalizedIngredientName.Contains(p.ProductName.ToLowerInvariant().Trim()));
            if (partialMatch != null)
            {
                _logger.LogDebug("Częściowe dopasowanie: '{IngredientName}' -> Product ID {ProductId}", ingredientName, partialMatch.Id);
                return partialMatch;
            }
            
            var reverseMatch = products.FirstOrDefault(p => 
                !string.IsNullOrWhiteSpace(p.ProductName) && 
                p.ProductName.ToLowerInvariant().Trim().Contains(normalizedIngredientName));
            if (reverseMatch != null)
            {
                _logger.LogDebug("Odwrotne dopasowanie: '{IngredientName}' -> Product ID {ProductId}", ingredientName, reverseMatch.Id);
                return reverseMatch;
            }
            
            _logger.LogDebug("Brak dopasowania dla składnika: '{IngredientName}'", ingredientName);
            return null;
        }
        
        private string FormatProductForPrompt(Product product)
        {
            var info = $"- {product.ProductName}";
            
            if (!string.IsNullOrEmpty(product.Brands))
            {
                info += $" (marka: {product.Brands})";
            }
            
            var nutritionParts = new List<string>();
            if (product.EnergyKcal100g.HasValue)
                nutritionParts.Add($"{product.EnergyKcal100g:F1} kcal/100g");
            if (product.Proteins100g.HasValue)
                nutritionParts.Add($"białko: {product.Proteins100g:F1}g/100g");
            if (product.Carbohydrates100g.HasValue)
                nutritionParts.Add($"węglowodany: {product.Carbohydrates100g:F1}g/100g");
            if (product.Fat100g.HasValue)
                nutritionParts.Add($"tłuszcze: {product.Fat100g:F1}g/100g");
            
            if (nutritionParts.Any())
            {
                info += $" | Wartości odżywcze: {string.Join(", ", nutritionParts)}";
            }
            
            return info;
        }
        
        private async Task<Dictionary<string, object?>> PreparePromptDataAsync(GenerateRecipeRequest request, List<Product> products)
        {
            var units = await _unitService.GetAllUnitsAsync();
            var unitNames = string.Join("\n", units.Select(u => u.Name));
            
            var productsInfo = "";
            if (products.Any())
            {
                productsInfo = string.Join("\n", products.Select(p => FormatProductForPrompt(p)));
            }
            
            var availableIngredientsText = request.AvailableIngredients.Any()
                ? string.Join("\n", request.AvailableIngredients) : "";
            
            var data = new Dictionary<string, object?>
            {
                ["availableIngredients"] = availableIngredientsText,
                ["productsFromDatabase"] = productsInfo,
                ["allowedUnits"] = unitNames,
                ["cuisineType"] = request.CuisineType,
                ["maxPreparationTimeMinutes"] = request.MaxPreparationTimeMinutes,
                ["additionalInstructions"] = request.AdditionalInstructions,
            };
            
            if (request.Preferences != null)
            {
                data["isVegan"] = request.Preferences.IsVegan;
                data["isVegetarian"] = request.Preferences.IsVegetarian;
                data["isGlutenFree"] = request.Preferences.IsGlutenFree;
                data["isLactoseFree"] = request.Preferences.IsLactoseFree;
                data["allergies"] = request.Preferences.Allergies.Any() 
                    ? string.Join(", ", request.Preferences.Allergies) 
                    : "brak";
                data["dislikedIngredients"] = request.Preferences.DislikedIngredients.Any()
                    ? string.Join(", ", request.Preferences.DislikedIngredients)
                    : "brak";
                
                
                if (request.Preferences.DailyCalorieGoal.HasValue)
                {
                    data["dailyCalorieGoal"] = request.Preferences.DailyCalorieGoal.Value;
                }
                
                if (request.Preferences.DailyProteinGoal.HasValue)
                {
                    data["dailyProteinGoal"] = request.Preferences.DailyProteinGoal.Value;
                }
                
                if (request.Preferences.DailyCarbohydrateGoal.HasValue)
                {
                    data["dailyCarbohydrateGoal"] = request.Preferences.DailyCarbohydrateGoal.Value;
                }
                
                if (request.Preferences.DailyFatGoal.HasValue)
                {
                    data["dailyFatGoal"] = request.Preferences.DailyFatGoal.Value;
                }
                
                if (!string.IsNullOrEmpty(request.MealType))
                {
                    data["mealType"] = request.MealType;
                    var mealTypeLower = request.MealType.ToLower();
                    if (mealTypeLower == MealType.Breakfast.ToString().ToLower()) {
                        data["Breakfast"] = request.MealType;

                    }
                    if (mealTypeLower == MealType.Dinner.ToString().ToLower()) {
                        data["Dinner"] = request.MealType;
                    }
                    if (mealTypeLower == MealType.Lunch.ToString().ToLower()) {
                        data["Lunch"] = request.MealType;
                    }
                    if (mealTypeLower == MealType.Snack.ToString().ToLower()) {
                        data["Snack"] = request.MealType;
                    }
                }
                else {
                    throw new Exception("MealType is required");
                }
                
                
                if (request.Preferences.TargetMealCalories.HasValue)
                {
                    data["targetMealCalories"] = request.Preferences.TargetMealCalories.Value;
                }
                else {
                    throw new Exception("TargetMealCalories is required in Preferences");
                }
                
                if (request.Preferences.TargetMealProtein.HasValue)
                {
                    data["targetMealProtein"] = request.Preferences.TargetMealProtein.Value;
                }
                else {
                    throw new Exception("TargetMealProtein is required in Preferences");
                }
                
                if (request.Preferences.TargetMealCarbohydrates.HasValue)
                {
                    data["targetMealCarbohydrates"] = request.Preferences.TargetMealCarbohydrates.Value;
                }
                else {
                    throw new Exception("TargetMealCarbohydrates is required in Preferences");
                }
                
                if (request.Preferences.TargetMealFat.HasValue)
                {
                    data["targetMealFat"] = request.Preferences.TargetMealFat.Value;
                }
                else {
                    throw new Exception("TargetMealFat is required in Preferences");
                }
            }
            return data;
        }
    }
