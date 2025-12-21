using inzynierka.MealPlans.Model;
using inzynierka.Users.Model;
using inzynierka.UserPreferences.Requests;

namespace inzynierka.UserPreferences.Extensions;

public static class FoodPreferencesUpdateExtensions
{
    public static void UpdateFrom(this FoodPreferences preferences, UpdateFoodPreferencesRequest request)
    {
        preferences.UpdateBasicPreferences(request);
        
        preferences.UpdateDailyGoals(request);
        
        preferences.UpdateHealthMetrics(request);
        
        preferences.UpdateMealDistributions(request);
    }

    private static void UpdateBasicPreferences(this FoodPreferences preferences, UpdateFoodPreferencesRequest request)
    {
        if (request.IsVegan.HasValue)
            preferences.IsVegan = request.IsVegan.Value;
        if (request.IsVegetarian.HasValue)
            preferences.IsVegetarian = request.IsVegetarian.Value;
        if (request.HasGlutenIntolerance.HasValue)
            preferences.HasGlutenIntolerance = request.HasGlutenIntolerance.Value;
        if (request.HasLactoseIntolerance.HasValue)
            preferences.HasLactoseIntolerance = request.HasLactoseIntolerance.Value;
        if (request.Allergies != null)
            preferences.Allergies = request.Allergies;
    }

    private static void UpdateHealthMetrics(this FoodPreferences preferences, UpdateFoodPreferencesRequest request)
    {
        bool shouldRecalculate = false;
        
        if (request.Age.HasValue)
        {
            preferences.Age = request.Age.Value;
            shouldRecalculate = true;
        }
        
        if (!string.IsNullOrEmpty(request.Gender) && 
            Enum.TryParse<Gender>(request.Gender, ignoreCase: true, out var gender))
        {
            preferences.Gender = gender;
            shouldRecalculate = true;
        }
        
        if (request.Weight.HasValue)
        {
            preferences.Weight = request.Weight.Value;
            shouldRecalculate = true;
        }
        
        if (request.Height.HasValue)
        {
            preferences.Height = request.Height.Value;
            shouldRecalculate = true;
        }
        
        if (!string.IsNullOrEmpty(request.ActivityLevel) && 
            Enum.TryParse<PhysicalActivityLevel>(request.ActivityLevel, ignoreCase: true, out var activityLevel))
        {
            preferences.ActivityLevel = activityLevel;
            shouldRecalculate = true;
        }
        
        if (!string.IsNullOrEmpty(request.FitnessGoal) && 
            Enum.TryParse<FitnessGoal>(request.FitnessGoal, ignoreCase: true, out var fitnessGoal))
        {
            preferences.FitnessGoal = fitnessGoal;
            shouldRecalculate = true;
        }
        
        if (shouldRecalculate)
        {
            bool hasManualCalories = request.DailyCalorieGoal.HasValue && request.DailyCalorieGoal.Value > 0;
            bool hasManualProtein = request.DailyProteinGoal.HasValue && request.DailyProteinGoal.Value > 0;
            bool hasManualCarbs = request.DailyCarbohydrateGoal.HasValue && request.DailyCarbohydrateGoal.Value > 0;
            bool hasManualFat = request.DailyFatGoal.HasValue && request.DailyFatGoal.Value > 0;
            
            if (!hasManualCalories) preferences.DailyCalorieGoal = 0;
            if (!hasManualProtein) preferences.DailyProteinGoal = 0;
            if (!hasManualCarbs) preferences.DailyCarbohydrateGoal = 0;
            if (!hasManualFat) preferences.DailyFatGoal = 0;
        }
        
        preferences.RecalculateNutritionalGoals();
    }
    
    private static void RecalculateNutritionalGoals(this FoodPreferences preferences)
    {
        if (!preferences.Age.HasValue || !preferences.Gender.HasValue || 
            !preferences.Weight.HasValue || !preferences.Height.HasValue ||
            !preferences.ActivityLevel.HasValue || !preferences.FitnessGoal.HasValue)
        {
            return;
        }
        
        var bmr = NutritionalCalculations.CalculateBMR(
            preferences.Age.Value, 
            preferences.Gender.Value, 
            preferences.Weight.Value, 
            preferences.Height.Value);
        
        var tdee = (int)(bmr * NutritionalCalculations.GetPALMultiplier(preferences.ActivityLevel.Value));
        var targetCalories = NutritionalCalculations.ApplyFitnessGoalAdjustment(tdee, preferences.FitnessGoal.Value);
        
        var caloriesForMacros = preferences.DailyCalorieGoal == 0 ? targetCalories : preferences.DailyCalorieGoal;
        var (protein, carbs, fat) = NutritionalCalculations.CalculateMacros(
            caloriesForMacros, 
            preferences.FitnessGoal.Value, 
            preferences.Weight.Value, 
            preferences.ActivityLevel.Value);
        
        preferences.DailyCalorieGoal = preferences.DailyCalorieGoal == 0 ? targetCalories : preferences.DailyCalorieGoal;
        preferences.DailyProteinGoal = preferences.DailyProteinGoal == 0 ? protein : preferences.DailyProteinGoal;
        preferences.DailyCarbohydrateGoal = preferences.DailyCarbohydrateGoal == 0 ? carbs : preferences.DailyCarbohydrateGoal;
        preferences.DailyFatGoal = preferences.DailyFatGoal == 0 ? fat : preferences.DailyFatGoal;
    }

    private static void UpdateDailyGoals(this FoodPreferences preferences, UpdateFoodPreferencesRequest request)
    {
        if (request.DailyProteinGoal.HasValue && request.DailyProteinGoal.Value > 0)
            preferences.DailyProteinGoal = request.DailyProteinGoal.Value;
        if (request.DailyCarbohydrateGoal.HasValue && request.DailyCarbohydrateGoal.Value > 0)
            preferences.DailyCarbohydrateGoal = request.DailyCarbohydrateGoal.Value;
        if (request.DailyFatGoal.HasValue && request.DailyFatGoal.Value > 0)
            preferences.DailyFatGoal = request.DailyFatGoal.Value;
        if (request.DailyCalorieGoal.HasValue && request.DailyCalorieGoal.Value > 0)
            preferences.DailyCalorieGoal = request.DailyCalorieGoal.Value;
    }

    private static void UpdateMealDistributions(this FoodPreferences preferences, UpdateFoodPreferencesRequest request)
    {
        preferences.Breakfast.UpdateFrom(request.Breakfast);
        preferences.Lunch.UpdateFrom(request.Lunch);
        preferences.Dinner.UpdateFrom(request.Dinner);
        preferences.Snack.UpdateFrom(request.Snack);
    }

    private static void UpdateFrom(
        this MealNutritionDistribution distribution, 
        UpdateMealNutritionDistributionRequest? request)
    {
        if (request == null) return;

        if (request.CaloriePercentage.HasValue)
            distribution.CaloriePercentage = request.CaloriePercentage.Value;
        if (request.ProteinPercentage.HasValue)
            distribution.ProteinPercentage = request.ProteinPercentage.Value;
        if (request.CarbohydratePercentage.HasValue)
            distribution.CarbohydratePercentage = request.CarbohydratePercentage.Value;
        if (request.FatPercentage.HasValue)
            distribution.FatPercentage = request.FatPercentage.Value;
    }
}

