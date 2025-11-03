using System.Text;
using inzynierka.AI.OpenAI.Config;
using inzynierka.AI.OpenAI.Model;
using inzynierka.AI.OpenAI.Services;
using inzynierka.Receipts.Requests;

namespace inzynierka.Receipts.Services;

public class RecipePromptBuilder : IRecipePromptBuilder
{
    private readonly IUnitService _unitService;
    private readonly IPromptConfigService _promptConfigService;
    private readonly ILogger<RecipePromptBuilder> _logger;
    private PromptConfig? _config;
    private readonly string _configPath;

    public RecipePromptBuilder(
        IUnitService unitService, 
        IPromptConfigService promptConfigService,
        ILogger<RecipePromptBuilder> logger,
        IConfiguration configuration)
    {
        _unitService = unitService;
        _promptConfigService = promptConfigService;
        _logger = logger;
        _configPath = configuration["Receipts:PromptConfigPath"] ?? 
                     Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Receipts/Config/prompt_config.json");
    }

    public async Task<List<OpenAIMessage>> BuildMessagesAsync(GenerateRecipeRequest request)
    {
        _config ??= await _promptConfigService.LoadConfigAsync(_configPath);

        var data = await PreparePromptDataAsync(request);

        var userPrompt = _promptConfigService.RenderPrompt(_config, data);

        var messages = new List<OpenAIMessage>
        {
            new OpenAIMessage("system", _config.SystemMessage),
            new OpenAIMessage("user", userPrompt)
        };

        return messages;
    }

    private async Task<Dictionary<string, object?>> PreparePromptDataAsync(GenerateRecipeRequest request)
    {
        var data = new Dictionary<string, object?>
        {
            ["availableIngredients"] = FormatIngredientsList(request.AvailableIngredients),
            ["cuisineType"] = request.CuisineType,
            ["desiredServings"] = request.DesiredServings,
            ["maxPreparationTimeMinutes"] = request.MaxPreparationTimeMinutes,
            ["additionalInstructions"] = request.AdditionalInstructions,
            
            ["allowedUnits"] = await FormatAllowedUnits()
        };

        if (request.Preferences != null)
        {
            data["isVegan"] = request.Preferences.IsVegan;
            data["isVegetarian"] = request.Preferences.IsVegetarian;
            data["isGlutenFree"] = request.Preferences.IsGlutenFree;
            data["isLactoseFree"] = request.Preferences.IsLactoseFree;
            data["maxCalories"] = request.Preferences.MaxCalories;
            data["allergies"] = request.Preferences.Allergies.Any() 
                ? string.Join(", ", request.Preferences.Allergies)
                : null;
            data["dislikedIngredients"] = request.Preferences.DislikedIngredients.Any()
                ? string.Join(", ", request.Preferences.DislikedIngredients)
                : null;
        }
        return data;
    }

    private string FormatIngredientsList(List<string> ingredients)
    {
        if (!ingredients.Any())
            return "Brak określonych składników";

        var builder = new StringBuilder();
        foreach (var ingredient in ingredients)
        {
            builder.AppendLine($"• {ingredient}");
        }
        return builder.ToString().TrimEnd();
    }

    private async Task<string> FormatAllowedUnits()
    {
        var units = await _unitService.GetAllUnitsAsync();
        var builder = new StringBuilder();
        
        foreach (var unit in units)
        {
            builder.AppendLine($"• {unit.Name}");
        }
        return builder.ToString().TrimEnd();
    }
}

