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
                
                // Apply fitness goal adjustment
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
        
        // Auto-calculate macros if fitness goal is set and we have calculated calories
        int targetCalories = preferences.DailyCalorieGoal > 0 
            ? preferences.DailyCalorieGoal 
            : (dailyCalories ?? 0);
            
        int dailyProtein = preferences.DailyProteinGoal;
        int dailyCarbs = preferences.DailyCarbohydrateGoal;
        int dailyFat = preferences.DailyFatGoal;
        
        // If fitness goal is set and we have weight and calculated calories, auto-calculate macros
        if (preferences.FitnessGoal.HasValue && preferences.Weight.HasValue && dailyCalories.HasValue)
        {
            var (protein, carbs, fat) = NutritionalCalculations.CalculateMacros(dailyCalories.Value, preferences.FitnessGoal.Value, preferences.Weight.Value);
            
            // Only override if user hasn't set custom values
            if (dailyProtein == 0) dailyProtein = protein;
            if (dailyCarbs == 0) dailyCarbs = carbs;
            if (dailyFat == 0) dailyFat = fat;
            if (targetCalories == 0) targetCalories = dailyCalories.Value;
        }
        
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

// Shared calculation methods for nutritional goals
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
    
    internal static (int protein, int carbs, int fat) CalculateMacros(int calories, FitnessGoal? goal, decimal weight)
    {
        // Default protein per kg body weight based on goal
        decimal proteinPerKg = goal switch
        {
            FitnessGoal.WeightLoss => 2.2m,      // Higher protein for muscle preservation
            FitnessGoal.Maintenance => 1.8m,
            FitnessGoal.WeightGain => 2.0m,      // Higher protein for muscle building
            _ => 1.8m
        };
        
        int proteinGrams = (int)(weight * proteinPerKg);
        int proteinCalories = proteinGrams * 4;
        
        // Fat: 25-30% of total calories
        decimal fatPercentage = goal switch
        {
            FitnessGoal.WeightLoss => 0.25m,
            FitnessGoal.Maintenance => 0.28m,
            FitnessGoal.WeightGain => 0.25m,
            _ => 0.28m
        };
        
        int fatCalories = (int)(calories * fatPercentage);
        int fatGrams = fatCalories / 9;
        
        // Carbs: remaining calories
        int carbCalories = calories - proteinCalories - fatCalories;
        int carbGrams = carbCalories / 4;
        
        return (proteinGrams, carbGrams, fatGrams);
    }
}

public static class FoodPreferencesUpdateExtensions
{
    public static void UpdateFrom(this FoodPreferences preferences, Requests.UpdateFoodPreferencesRequest request, ILogger? logger = null)
    {
        preferences.UpdateBasicPreferences(request);
        preferences.UpdateHealthMetrics(request, logger);
        preferences.UpdateDailyGoals(request);
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
        if (request.Age.HasValue)
            preferences.Age = request.Age.Value;
        
        if (!string.IsNullOrEmpty(request.Gender))
        {
            if (Enum.TryParse<Gender>(request.Gender, ignoreCase: true, out var gender))
                preferences.Gender = gender;
            else
                logger?.LogWarning("Invalid Gender value: {Gender}", request.Gender);
        }
        
        if (request.Weight.HasValue)
            preferences.Weight = request.Weight.Value;
        if (request.Height.HasValue)
            preferences.Height = request.Height.Value;
        
        if (!string.IsNullOrEmpty(request.ActivityLevel))
        {
            if (Enum.TryParse<PhysicalActivityLevel>(request.ActivityLevel, ignoreCase: true, out var activityLevel))
                preferences.ActivityLevel = activityLevel;
            else
                logger?.LogWarning("Invalid ActivityLevel value: {ActivityLevel}", request.ActivityLevel);
        }
        
        if (!string.IsNullOrEmpty(request.FitnessGoal))
        {
            if (Enum.TryParse<FitnessGoal>(request.FitnessGoal, ignoreCase: true, out var fitnessGoal))
                preferences.FitnessGoal = fitnessGoal;
            else
                logger?.LogWarning("Invalid FitnessGoal value: {FitnessGoal}", request.FitnessGoal);
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
        var (protein, carbs, fat) = NutritionalCalculations.CalculateMacros(targetCalories, preferences.FitnessGoal.Value, preferences.Weight.Value);
        
        // Update preferences (only if not manually set by user)
        if (preferences.DailyCalorieGoal == 0)
        {
            preferences.DailyCalorieGoal = targetCalories;
            logger?.LogInformation("Auto-calculated DailyCalorieGoal: {Calories} kcal", targetCalories);
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
        if (request.DailyProteinGoal.HasValue)
            preferences.DailyProteinGoal = request.DailyProteinGoal.Value;
        if (request.DailyCarbohydrateGoal.HasValue)
            preferences.DailyCarbohydrateGoal = request.DailyCarbohydrateGoal.Value;
        if (request.DailyFatGoal.HasValue)
            preferences.DailyFatGoal = request.DailyFatGoal.Value;
        if (request.DailyCalorieGoal.HasValue)
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

