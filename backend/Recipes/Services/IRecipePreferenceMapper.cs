using inzynierka.Recipes.Model.RecipeModel;
using inzynierka.Users.Responses;

namespace inzynierka.Recipes.Services;

 
public interface IRecipePreferenceMapper
{
    DietaryPreferences? MapFromUserPreferences(FoodPreferencesDto? userPreferences);
    void ApplyMealTypeGoals(DietaryPreferences preferences, string mealType, FoodPreferencesDto userPreferences);
}