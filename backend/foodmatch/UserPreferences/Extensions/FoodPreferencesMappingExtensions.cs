using inzynierka.MealPlans.Model;
using inzynierka.Users.Model;
using inzynierka.UserPreferences.Responses;
using Microsoft.Extensions.Logging;

namespace inzynierka.UserPreferences.Extensions;

public static class FoodPreferencesMappingExtensions
{
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
            FitnessGoal.WeightLoss => (int)(tdee)-500,
            FitnessGoal.Maintenance => tdee,
            FitnessGoal.WeightGain => (int)(tdee)+500,
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
        if (proteinPerKg < 1.2m) proteinPerKg = 1.2m;
        if (proteinPerKg > 2.2m) proteinPerKg = 2.2m;

        int proteinGrams = (int)Math.Round(weight * proteinPerKg);
        int proteinCalories = proteinGrams * 4;

        decimal fatPercentage = activityLevel switch
        {
            PhysicalActivityLevel.Sedentary => 0.30m,
            PhysicalActivityLevel.LightlyActive => 0.28m,
            PhysicalActivityLevel.ModeratelyActive => 0.25m,
            PhysicalActivityLevel.VeryActive => 0.25m,
            PhysicalActivityLevel.ExtraActive => 0.22m,
            _ => 0.28m
        };

        fatPercentage = goal switch
        {
            FitnessGoal.WeightLoss => Math.Max(0.22m, fatPercentage - 0.02m),
            FitnessGoal.WeightGain => Math.Min(0.32m, fatPercentage + 0.02m),
            _ => fatPercentage
        };

        int fatCalories = (int)Math.Round(calories * fatPercentage);
        int fatGrams = (int)Math.Round(fatCalories / 9.0);

        int carbCalories = calories - proteinCalories - fatCalories;
        if (carbCalories < 0) carbCalories = 0;
        int carbGrams = (int)Math.Round(carbCalories / 4.0);

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

