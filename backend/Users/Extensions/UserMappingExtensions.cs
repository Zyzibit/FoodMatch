using inzynierka.MealPlans.Model;
using inzynierka.Users.Model;
using inzynierka.Users.Responses;
using Microsoft.Extensions.Logging;

namespace inzynierka.Users.Extensions;

public static class UserMappingExtensions
{
    public static UserDto ToDto(this User user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }

    public static FoodPreferencesDto ToDto(this FoodPreferences preferences)
    {
        int? bmr = null;
        int? dailyCalories = null;
        
        if (preferences.Age.HasValue && preferences.Gender.HasValue && 
            preferences.Weight.HasValue && preferences.Height.HasValue)
        {
            bmr = NutritionalCalculations.CalculateBMR(preferences.Age.Value, preferences.Gender.Value, 
                             preferences.Weight.Value, preferences.Height.Value);
            
            if (preferences.ActivityLevel.HasValue && bmr.HasValue)
            {
                var tdee = (int)(bmr.Value * NutritionalCalculations.GetPALMultiplier(preferences.ActivityLevel.Value));
                
                if (preferences.FitnessGoal.HasValue)
                {
                    dailyCalories = NutritionalCalculations.ApplyFitnessGoalAdjustment(tdee, preferences.FitnessGoal.Value);
                }
                else
                {
                    dailyCalories = tdee;
                }
            }
        }
        int targetCalories = preferences.DailyCalorieGoal;
        int dailyProtein = preferences.DailyProteinGoal;
        int dailyCarbs = preferences.DailyCarbohydrateGoal;
        int dailyFat = preferences.DailyFatGoal;
        
        return new FoodPreferencesDto
        {
            IsVegan = preferences.IsVegan,
            IsVegetarian = preferences.IsVegetarian,
            HasGlutenIntolerance = preferences.HasGlutenIntolerance,
            HasLactoseIntolerance = preferences.HasLactoseIntolerance,
            Allergies = preferences.Allergies,
            Age = preferences.Age,
            Gender = preferences.Gender?.ToString(),
            Weight = preferences.Weight,
            Height = preferences.Height,
            ActivityLevel = preferences.ActivityLevel?.ToString(),
            FitnessGoal = preferences.FitnessGoal?.ToString(),
            DailyCarbohydrateGoal = dailyCarbs,
            DailyProteinGoal = dailyProtein,
            DailyFatGoal = dailyFat,
            DailyCalorieGoal = targetCalories > 0 ? targetCalories : preferences.DailyCalorieGoal,
            CalculatedBMR = bmr,
            CalculatedDailyCalories = dailyCalories,
            Breakfast = CreateMealNutrition(
                preferences.Breakfast,
                targetCalories,
                dailyProtein,
                dailyCarbs,
                dailyFat
            ),
            Lunch = CreateMealNutrition(
                preferences.Lunch,
                targetCalories,
                dailyProtein,
                dailyCarbs,
                dailyFat
            ),
            Dinner = CreateMealNutrition(
                preferences.Dinner,
                targetCalories,
                dailyProtein,
                dailyCarbs,
                dailyFat
            ),
            Snack = CreateMealNutrition(
                preferences.Snack,
                targetCalories,
                dailyProtein,
                dailyCarbs,
                dailyFat
            )
        };
    }
    
    private static MealNutritionDto CreateMealNutrition(
        MealNutritionDistribution distribution,
        int targetCalories,
        int dailyProteinGoal,
        int dailyCarbohydrateGoal,
        int dailyFatGoal)
    {
        return new MealNutritionDto
        {
            CaloriePercentage = distribution.CaloriePercentage,
            ProteinPercentage = distribution.ProteinPercentage,
            CarbohydratePercentage = distribution.CarbohydratePercentage,
            FatPercentage = distribution.FatPercentage,
            CaloriesGoal = targetCalories > 0 ? (int)(targetCalories * distribution.CaloriePercentage / 100.0) : null,
            ProteinGoal = dailyProteinGoal > 0 ? (int)(dailyProteinGoal * distribution.ProteinPercentage / 100.0) : null,
            CarbohydrateGoal = dailyCarbohydrateGoal > 0 ? (int)(dailyCarbohydrateGoal * distribution.CarbohydratePercentage / 100.0) : null,
            FatGoal = dailyFatGoal > 0 ? (int)(dailyFatGoal * distribution.FatPercentage / 100.0) : null
        };
    }
    
} 
internal static class NutritionalCalculations
{
    internal static int CalculateBMR(int age, Gender gender, decimal weight, decimal height)
    {
        decimal bmr = (10 * weight) + (6.25m * height) - (5 * age);
        
        if (gender == Gender.Male)
        {
            bmr += 5;
        }
        else if (gender == Gender.Female)
        {
            bmr -= 161;
        }
        
        return (int)Math.Round(bmr);
    }
    
    internal static decimal GetPALMultiplier(PhysicalActivityLevel level)
    {
        return level switch
        {
            PhysicalActivityLevel.Sedentary => 1.2m,
            PhysicalActivityLevel.LightlyActive => 1.375m,
            PhysicalActivityLevel.ModeratelyActive => 1.55m,
            PhysicalActivityLevel.VeryActive => 1.725m,
            PhysicalActivityLevel.ExtraActive => 1.9m,
            _ => 1.2m
        };
    }
    
    internal static int ApplyFitnessGoalAdjustment(int tdee, FitnessGoal goal)
    {
        return goal switch
        {
            FitnessGoal.WeightLoss => (int)(tdee * 0.8), // -20% deficit
            FitnessGoal.Maintenance => tdee,
            FitnessGoal.WeightGain => (int)(tdee * 1.15), // +15% surplus
            _ => tdee
        };
    }
    
    internal static (int protein, int carbs, int fat) CalculateMacros(int calories, FitnessGoal? goal, decimal weight, PhysicalActivityLevel? activityLevel)
    {
        decimal baseProteinPerKg = goal switch
        {
            FitnessGoal.WeightLoss => 2.0m,      
            FitnessGoal.Maintenance => 1.6m,
            FitnessGoal.WeightGain => 1.8m,
            _ => 1.6m
        };

        // Korekta białka wg poziomu aktywności (łagodniejsze)
        decimal activityProteinAdj = activityLevel switch
        {
            PhysicalActivityLevel.Sedentary => -0.2m,
            PhysicalActivityLevel.LightlyActive => 0.0m,
            PhysicalActivityLevel.ModeratelyActive => 0.1m,
            PhysicalActivityLevel.VeryActive => 0.15m,
            PhysicalActivityLevel.ExtraActive => 0.2m,
            _ => 0.0m
        };
        decimal proteinPerKg = baseProteinPerKg + activityProteinAdj;
        // Bezpieczne widełki: min 1.2 g/kg, max 2.2 g/kg
        if (proteinPerKg < 1.2m) proteinPerKg = 1.2m;
        if (proteinPerKg > 2.2m) proteinPerKg = 2.2m;

        int proteinGrams = (int)Math.Round(weight * proteinPerKg);
        int proteinCalories = proteinGrams * 4;

        // Tłuszcze: procent kalorii zależny od aktywności (więcej tłuszczu przy niskiej aktywności)
        decimal fatPercentage = activityLevel switch
        {
            PhysicalActivityLevel.Sedentary => 0.30m,
            PhysicalActivityLevel.LightlyActive => 0.28m,
            PhysicalActivityLevel.ModeratelyActive => 0.25m,
            PhysicalActivityLevel.VeryActive => 0.25m,
            PhysicalActivityLevel.ExtraActive => 0.22m,
            _ => 0.28m
        };

        // Dodatkowa delikatna korekta wg celu
        fatPercentage = goal switch
        {
            FitnessGoal.WeightLoss => Math.Max(0.22m, fatPercentage - 0.02m),
            FitnessGoal.WeightGain => Math.Min(0.32m, fatPercentage + 0.02m),
            _ => fatPercentage
        };

        int fatCalories = (int)Math.Round(calories * fatPercentage);
        int fatGrams = (int)Math.Round(fatCalories / 9.0);

        // Węglowodany: pozostałe kalorie po białku i tłuszczu
        int carbCalories = calories - proteinCalories - fatCalories;
        if (carbCalories < 0) carbCalories = 0; // zabezpieczenie
        int carbGrams = (int)Math.Round(carbCalories / 4.0);

        // Minimalne węglowodany wg aktywności
        int minCarbGrams = activityLevel switch
        {
            PhysicalActivityLevel.Sedentary => 100,
            PhysicalActivityLevel.LightlyActive => 125,
            PhysicalActivityLevel.ModeratelyActive => 150,
            PhysicalActivityLevel.VeryActive => 200,
            PhysicalActivityLevel.ExtraActive => 250,
            _ => 125
        };

        if (carbGrams < minCarbGrams)
        {
            int requiredCarbCalories = minCarbGrams * 4;
            int delta = requiredCarbCalories - carbCalories;
            if (delta > 0)
            {
                int newFatCalories = fatCalories - delta;
                if (newFatCalories < 0) newFatCalories = 0;
                fatCalories = newFatCalories;
                fatGrams = (int)Math.Round(fatCalories / 9.0);
                carbCalories = calories - proteinCalories - fatCalories;
                if (carbCalories < 0) carbCalories = 0;
                carbGrams = (int)Math.Round(carbCalories / 4.0);
            }
        }

        return (proteinGrams, carbGrams, fatGrams);
    }
}

public static class FoodPreferencesUpdateExtensions
{
    public static void UpdateFrom(this FoodPreferences preferences, Requests.UpdateFoodPreferencesRequest request, ILogger? logger = null)
    {
        preferences.UpdateBasicPreferences(request);
        
        preferences.UpdateDailyGoals(request);
        
        preferences.UpdateHealthMetrics(request, logger);
        
        preferences.UpdateMealDistributions(request);
    }

    private static void UpdateBasicPreferences(this FoodPreferences preferences, Requests.UpdateFoodPreferencesRequest request)
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

    private static void UpdateHealthMetrics(this FoodPreferences preferences, Requests.UpdateFoodPreferencesRequest request, ILogger? logger = null)
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
        
        // If key parameters changed, ALWAYS reset goals to 0 to force recalculation
        // This ensures values are recalculated with new parameters
        // (UpdateDailyGoals will override these if user explicitly provided values)
        if (shouldRecalculate)
        {
            // Check if user provided manual values in THIS request
            bool hasManualCalories = request.DailyCalorieGoal.HasValue && request.DailyCalorieGoal.Value > 0;
            bool hasManualProtein = request.DailyProteinGoal.HasValue && request.DailyProteinGoal.Value > 0;
            bool hasManualCarbs = request.DailyCarbohydrateGoal.HasValue && request.DailyCarbohydrateGoal.Value > 0;
            bool hasManualFat = request.DailyFatGoal.HasValue && request.DailyFatGoal.Value > 0;
            
            // Reset to 0 to force recalculation (unless manually set in THIS request)
            if (!hasManualCalories) preferences.DailyCalorieGoal = 0;
            if (!hasManualProtein) preferences.DailyProteinGoal = 0;
            if (!hasManualCarbs) preferences.DailyCarbohydrateGoal = 0;
            if (!hasManualFat) preferences.DailyFatGoal = 0;
            
            logger?.LogDebug("Health metrics changed - will recalculate nutritional goals");
        }
        
        // Auto-calculate goals if we have all required data and FitnessGoal is set
        preferences.RecalculateNutritionalGoals(logger);
    }
    
    private static void RecalculateNutritionalGoals(this FoodPreferences preferences, ILogger? logger = null)
    {
        // Only auto-calculate if we have all required data and FitnessGoal
        if (!preferences.Age.HasValue || !preferences.Gender.HasValue || 
            !preferences.Weight.HasValue || !preferences.Height.HasValue ||
            !preferences.ActivityLevel.HasValue || !preferences.FitnessGoal.HasValue)
        {
            logger?.LogDebug("Skipping auto-calculation: missing required data for nutritional goals");
            return;
        }
        
        // Calculate BMR
        var bmr = NutritionalCalculations.CalculateBMR(
            preferences.Age.Value, 
            preferences.Gender.Value, 
            preferences.Weight.Value, 
            preferences.Height.Value);
        
        // Calculate TDEE
        var tdee = (int)(bmr * NutritionalCalculations.GetPALMultiplier(preferences.ActivityLevel.Value));
        
        // Adjust for fitness goal
        var targetCalories = NutritionalCalculations.ApplyFitnessGoalAdjustment(tdee, preferences.FitnessGoal.Value);
        
        // Calculate macros
        var (protein, carbs, fat) = NutritionalCalculations.CalculateMacros(targetCalories, preferences.FitnessGoal.Value, preferences.Weight.Value, preferences.ActivityLevel.Value);
        
        // ALWAYS recalculate and save to database (unless manually overridden with > 0 value)
        // This ensures breakfast/lunch/dinner goals are always up-to-date
        if (preferences.DailyCalorieGoal == 0)
        {
            preferences.DailyCalorieGoal = targetCalories;
            logger?.LogInformation("Auto-calculated DailyCalorieGoal: {Calories} kcal", targetCalories);
        }
        else
        {
            // User has manual override - recalculate based on manual calorie goal
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

    private static void UpdateDailyGoals(this FoodPreferences preferences, Requests.UpdateFoodPreferencesRequest request)
    {
        // Only update if user explicitly provided a positive value (manual override)
        // If null or 0, leave current value (will be auto-calculated if still 0)
        if (request.DailyProteinGoal.HasValue && request.DailyProteinGoal.Value > 0)
            preferences.DailyProteinGoal = request.DailyProteinGoal.Value;
        if (request.DailyCarbohydrateGoal.HasValue && request.DailyCarbohydrateGoal.Value > 0)
            preferences.DailyCarbohydrateGoal = request.DailyCarbohydrateGoal.Value;
        if (request.DailyFatGoal.HasValue && request.DailyFatGoal.Value > 0)
            preferences.DailyFatGoal = request.DailyFatGoal.Value;
        if (request.DailyCalorieGoal.HasValue && request.DailyCalorieGoal.Value > 0)
            preferences.DailyCalorieGoal = request.DailyCalorieGoal.Value;
    }

    private static void UpdateMealDistributions(this FoodPreferences preferences, Requests.UpdateFoodPreferencesRequest request)
    {
        preferences.Breakfast.UpdateFrom(request.Breakfast);
        preferences.Lunch.UpdateFrom(request.Lunch);
        preferences.Dinner.UpdateFrom(request.Dinner);
        preferences.Snack.UpdateFrom(request.Snack);
    }

    private static void UpdateFrom(
        this MealNutritionDistribution distribution, 
        Requests.UpdateMealNutritionDistributionRequest? request)
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
