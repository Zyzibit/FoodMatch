using inzynierka.MealPlans.Model;
using inzynierka.Users.Model;
using inzynierka.UserPreferences.Requests;
using Microsoft.Extensions.Logging;

namespace inzynierka.UserPreferences.Extensions;

public static class FoodPreferencesUpdateExtensions
{
    public static void UpdateFrom(this FoodPreferences preferences, UpdateFoodPreferencesRequest request, ILogger? logger = null)
    {
        preferences.UpdateBasicPreferences(request);
        
        preferences.UpdateDailyGoals(request);
        
        preferences.UpdateHealthMetrics(request, logger);
        
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

    private static void UpdateHealthMetrics(this FoodPreferences preferences, UpdateFoodPreferencesRequest request, ILogger? logger = null)
    {
        bool shouldRecalculate = false;
        
        if (request.Age.HasValue)
        {
            preferences.Age = request.Age.Value;
            shouldRecalculate = true;
        }
        
        if (!string.IsNullOrEmpty(request.Gender))
        {
            if (Enum.TryParse<Gender>(request.Gender, ignoreCase: true, out var gender))
            {
                preferences.Gender = gender;
                shouldRecalculate = true;
            }
            else
                logger?.LogWarning("Invalid Gender value: {Gender}", request.Gender);
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
        
        if (!string.IsNullOrEmpty(request.ActivityLevel))
        {
            if (Enum.TryParse<PhysicalActivityLevel>(request.ActivityLevel, ignoreCase: true, out var activityLevel))
            {
                preferences.ActivityLevel = activityLevel;
                shouldRecalculate = true;
            }
            else
                logger?.LogWarning("Invalid ActivityLevel value: {ActivityLevel}", request.ActivityLevel);
        }
        
        if (!string.IsNullOrEmpty(request.FitnessGoal))
        {
            if (Enum.TryParse<FitnessGoal>(request.FitnessGoal, ignoreCase: true, out var fitnessGoal))
            {
                preferences.FitnessGoal = fitnessGoal;
                shouldRecalculate = true;
            }
            else
                logger?.LogWarning("Invalid FitnessGoal value: {FitnessGoal}", request.FitnessGoal);
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
            
            logger?.LogDebug("Health metrics changed - will recalculate nutritional goals");
        }
        
        preferences.RecalculateNutritionalGoals(logger);
    }
    
    private static void RecalculateNutritionalGoals(this FoodPreferences preferences, ILogger? logger = null)
    {
        if (!preferences.Age.HasValue || !preferences.Gender.HasValue || 
            !preferences.Weight.HasValue || !preferences.Height.HasValue ||
            !preferences.ActivityLevel.HasValue || !preferences.FitnessGoal.HasValue)
        {
            logger?.LogDebug("Skipping auto-calculation: missing required data for nutritional goals");
            return;
        }
        
        var bmr = NutritionalCalculations.CalculateBMR(
            preferences.Age.Value, 
            preferences.Gender.Value, 
            preferences.Weight.Value, 
            preferences.Height.Value);
        
        var tdee = (int)(bmr * NutritionalCalculations.GetPALMultiplier(preferences.ActivityLevel.Value));
        
        var targetCalories = NutritionalCalculations.ApplyFitnessGoalAdjustment(tdee, preferences.FitnessGoal.Value);
        
        var (protein, carbs, fat) = NutritionalCalculations.CalculateMacros(targetCalories, preferences.FitnessGoal.Value, preferences.Weight.Value, preferences.ActivityLevel.Value);
        
        if (preferences.DailyCalorieGoal == 0)
        {
            preferences.DailyCalorieGoal = targetCalories;
            logger?.LogInformation("Auto-calculated DailyCalorieGoal: {Calories} kcal", targetCalories);
        }
        else
        {
            var manualCalories = preferences.DailyCalorieGoal;
            var (manualProtein, manualCarbs, manualFat) = NutritionalCalculations.CalculateMacros(manualCalories, preferences.FitnessGoal.Value, preferences.Weight.Value, preferences.ActivityLevel.Value);
            
            if (preferences.DailyProteinGoal == 0)
            {
                preferences.DailyProteinGoal = manualProtein;
                logger?.LogInformation("Auto-calculated DailyProteinGoal based on manual calories: {Protein}g", manualProtein);
            }
            if (preferences.DailyCarbohydrateGoal == 0)
            {
                preferences.DailyCarbohydrateGoal = manualCarbs;
                logger?.LogInformation("Auto-calculated DailyCarbohydrateGoal based on manual calories: {Carbs}g", manualCarbs);
            }
            if (preferences.DailyFatGoal == 0)
            {
                preferences.DailyFatGoal = manualFat;
                logger?.LogInformation("Auto-calculated DailyFatGoal based on manual calories: {Fat}g", manualFat);
            }
            return;
        }
        
        if (preferences.DailyProteinGoal == 0)
        {
            preferences.DailyProteinGoal = protein;
            logger?.LogInformation("Auto-calculated DailyProteinGoal: {Protein}g", protein);
        }
        
        if (preferences.DailyCarbohydrateGoal == 0)
        {
            preferences.DailyCarbohydrateGoal = carbs;
            logger?.LogInformation("Auto-calculated DailyCarbohydrateGoal: {Carbs}g", carbs);
        }
        
        if (preferences.DailyFatGoal == 0)
        {
            preferences.DailyFatGoal = fat;
            logger?.LogInformation("Auto-calculated DailyFatGoal: {Fat}g", fat);
        }
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

