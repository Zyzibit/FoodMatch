using System.Text;
using inzynierka.AI.Contracts.Models;
using inzynierka.Receipts.Services;

namespace inzynierka.AI.OpenAI.PromptBuilders;

public interface IRecipePromptBuilder
{
    Task<string> BuildPromptAsync(GenerateRecipeRequest request);
}

public class RecipePromptBuilder : IRecipePromptBuilder
{
    private readonly IUnitService _unitService;

    public RecipePromptBuilder(IUnitService unitService)
    {
        _unitService = unitService;
    }

    public async Task<string> BuildPromptAsync(GenerateRecipeRequest request)
    {
        var builder = new StringBuilder();
        
        builder.AppendLine("# ZADANIE");
        builder.AppendLine("Wygeneruj spójny i smaczny przepis kulinarny zgodnie z poniższymi parametrami.");
        builder.AppendLine();

        AppendIngredientsSection(builder, request.AvailableIngredients);
        AppendConstraintsSection(builder, request);
        AppendPreferencesSection(builder, request.Preferences);
        await AppendJsonSchemaSection(builder);

        return builder.ToString();
    }

    private void AppendIngredientsSection(StringBuilder builder, List<string> ingredients)
    {
        if (!ingredients.Any()) return;

        builder.AppendLine("## DOSTĘPNE SKŁADNIKI");
        foreach (var ingredient in ingredients)
        {
            builder.AppendLine($"• {ingredient}");
        }
        
        builder.AppendLine();
        builder.AppendLine("### ZASADY WYKORZYSTANIA SKŁADNIKÓW:");
        builder.AppendLine("✓ Wybierz składniki, które harmonijnie ze sobą współgrają");
        builder.AppendLine("✓ Pomiń składniki nieodpowiednie do danego dania (np. napoje, przekąski)");
        builder.AppendLine("✓ Możesz użyć tylko części podanych składników");
        builder.AppendLine("✓ Możesz zasugerować podstawowe dodatki (sól, pieprz, olej) jeśli są konieczne");
        builder.AppendLine();
    }

    private void AppendConstraintsSection(StringBuilder builder, GenerateRecipeRequest request)
    {
        var hasConstraints = !string.IsNullOrWhiteSpace(request.CuisineType) ||
                           request.DesiredServings.HasValue ||
                           request.MaxPreparationTimeMinutes.HasValue ||
                           !string.IsNullOrWhiteSpace(request.AdditionalInstructions);

        if (!hasConstraints) return;

        builder.AppendLine("## WYMAGANIA");
        
        if (!string.IsNullOrWhiteSpace(request.CuisineType))
            builder.AppendLine($"• Kuchnia: {request.CuisineType}");
        
        if (request.DesiredServings.HasValue)
            builder.AppendLine($"• Liczba porcji: {request.DesiredServings}");
        
        if (request.MaxPreparationTimeMinutes.HasValue)
            builder.AppendLine($"• Maksymalny czas przygotowania: {request.MaxPreparationTimeMinutes} min");
        
        if (!string.IsNullOrWhiteSpace(request.AdditionalInstructions))
            builder.AppendLine($"• Dodatkowe wskazówki: {request.AdditionalInstructions}");
        
        builder.AppendLine();
    }

    private void AppendPreferencesSection(StringBuilder builder, DietaryPreferences? preferences)
    {
        if (preferences == null) return;

        var hasDietaryRestrictions = preferences.IsVegetarian || preferences.IsVegan ||
                                    preferences.IsGlutenFree || preferences.IsLactoseFree ||
                                    preferences.Allergies.Any() || preferences.DislikedIngredients.Any() ||
                                    preferences.MaxCalories.HasValue;

        if (!hasDietaryRestrictions) return;

        builder.AppendLine("## OGRANICZENIA DIETETYCZNE");
        
        var restrictions = new List<string>();
        
        if (preferences.IsVegan)
            restrictions.Add("Dieta wegańska (bez produktów pochodzenia zwierzęcego)");
        else if (preferences.IsVegetarian)
            restrictions.Add("Dieta wegetariańska (bez mięsa i ryb)");
        
        if (preferences.IsGlutenFree)
            restrictions.Add("Bez glutenu");
        
        if (preferences.IsLactoseFree)
            restrictions.Add("Bez laktozy");
        
        if (preferences.MaxCalories.HasValue)
            restrictions.Add($"Maks. {preferences.MaxCalories} kcal na porcję");

        foreach (var restriction in restrictions)
        {
            builder.AppendLine($"• {restriction}");
        }

        if (preferences.Allergies.Any())
        {
            builder.AppendLine($"• ALERGENY DO UNIKNIĘCIA: {string.Join(", ", preferences.Allergies)}");
        }

        if (preferences.DislikedIngredients.Any())
        {
            builder.AppendLine($"• Składniki do pominięcia: {string.Join(", ", preferences.DislikedIngredients)}");
        }

        builder.AppendLine();
    }

    private async Task AppendJsonSchemaSection(StringBuilder builder)
    {
        builder.AppendLine("## FORMAT ODPOWIEDZI");
        builder.AppendLine("Zwróć przepis WYŁĄCZNIE jako poprawny JSON (bez markdown, bez komentarzy):");
        builder.AppendLine();
        builder.AppendLine("```json");
        builder.AppendLine(@"{
  ""title"": ""Atrakcyjna nazwa przepisu"",
  ""description"": ""Krótki, zachęcający opis (1-2 zdania)"",
  ""servings"": 2,
  ""preparationTimeMinutes"": 30,
  ""ingredients"": [
    {
      ""name"": ""Nazwa składnika"",
      ""quantity"": 200,
      ""unit"": ""g"",
      ""estimatedCalories"": 40,
      ""estimatedProteins"": 3,
      ""estimatedCarbohydrates"": 8,
      ""estimatedFats"": 0
    }
  ],
  ""instructions"": ""Krok 1: ...\nKrok 2: ...\nKrok 3: ..."",
  ""totalWeightGrams"": 800,
  ""estimatedCalories"": 150,
  ""estimatedProtein"": 12,
  ""estimatedCarbohydrates"": 20,
  ""estimatedFats"": 5
}");
        builder.AppendLine("```");
        builder.AppendLine();
        builder.AppendLine("⚠️ **KLUCZOWE INFORMACJE**:");
        builder.AppendLine("• **totalWeightGrams** - Całkowita waga gotowego dania w gramach (suma wszystkich składników po przygotowaniu)");
        builder.AppendLine("• **Wartości odżywcze na poziomie przepisu** (kalorie, białko, węglowodany, tłuszcze) - oblicz **NA 100g GOTOWEGO DANIA**");
        builder.AppendLine("• **Wartości odżywcze dla każdego składnika** - oblicz dla 100g składnika przed przygotowaniem");
        builder.AppendLine("• **Czyli podaj wartości odżywcze na 100g składnika w jego surowej formie**");
        builder.AppendLine();
        builder.AppendLine("### DODATKOWE WYTYCZNE:");
        builder.AppendLine("• Używaj tylko dozwolonych jednostek miary podanych poniżej");
        builder.AppendLine("• Określ totalWeightGrams jako wagę wszystkich składników przed przygotowaniem ");
        builder.AppendLine("• Suma wartości odżywczych wszystkich składników powinna być zbliżona do wartości przepisu");
        builder.AppendLine();
    
        await AppendUnitConstraints(builder);
        AppendNutritionalGuidelines(builder);
    }

    private async Task AppendUnitConstraints(StringBuilder builder)
    {
        var units = await _unitService.GetAllUnitsAsync();
        
        if (!units.Any()) return;

        builder.AppendLine("### DOZWOLONE JEDNOSTKI MIARY:");
        
        var unitsGrouped = units
            .GroupBy(u => GetUnitCategory(u.Name))
            .OrderBy(g => g.Key);

        foreach (var group in unitsGrouped)
        {
            builder.Append($"**{group.Key}**: ");
            builder.AppendLine(string.Join(", ", group.Select(u => u.Name)));
        }
        
        builder.AppendLine();
        builder.AppendLine("⚠️ WAŻNE: Używaj TYLKO jednostek z powyższej listy!");
        builder.AppendLine();
    }

    private string GetUnitCategory(string unitName)
    {
        return unitName.ToLower() switch
        {
            "g" or "kg" or "dag" => "Waga",
            "ml" or "l" => "Objętość",
            "łyżka" or "łyżeczka" or "szczypta" => "Kuchenne",
            "sztuka" or "pęczek" or "ząbek" or "plaster" => "Sztuki",
            _ => "Inne"
        };
    }

    private void AppendNutritionalGuidelines(StringBuilder builder)
    {
        builder.AppendLine("### WYTYCZNE WARTOŚCI ODŻYWCZYCH:");
        builder.AppendLine("• Podaj wartości na 100g gotowego dania");
        builder.AppendLine("• Szacuj konserwatywnie na podstawie składników");
        builder.AppendLine("• Wszystkie wartości jako liczby całkowite");
        builder.AppendLine();
    }
}
