using inzynierka.MealPlans.Model;
using inzynierka.MealPlans.Constants;
using inzynierka.MealPlans.Responses;

namespace inzynierka.MealPlans.Extensions;

public static class MealPlanExtensions
{
    public static DateTime GetUtcDate(this MealPlan mealPlan)
    {
        ArgumentNullException.ThrowIfNull(mealPlan);
        var date = mealPlan.Date;
        return date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime();
    }
    public static bool IsForDate(this MealPlan mealPlan, DateTime date)
    {
        ArgumentNullException.ThrowIfNull(mealPlan);
        var planDate = mealPlan.GetUtcDate().Date;
        var otherDate = (date.Kind == DateTimeKind.Unspecified ? DateTime.SpecifyKind(date, DateTimeKind.Utc) : date.ToUniversalTime()).Date;
        return planDate == otherDate;
    }
    
    public static GetMealPlansForDateResponse ToGetMealPlansForDateResponse(
        this IEnumerable<MealPlanDto> mealPlanDtos, 
        DateTime date)
    {
        ArgumentNullException.ThrowIfNull(mealPlanDtos);

        var mealPlanList = mealPlanDtos.ToList();

        return new GetMealPlansForDateResponse
        {
            Success = true,
            MealPlans = mealPlanList,
            TotalCalories = mealPlanList.Sum(mp => mp.Recipe?.Calories ?? 0),
            TotalProteins = mealPlanList.Sum(mp => mp.Recipe?.Proteins ?? 0),
            TotalCarbohydrates = mealPlanList.Sum(mp => mp.Recipe?.Carbohydrates ?? 0),
            TotalFats = mealPlanList.Sum(mp => mp.Recipe?.Fats ?? 0),
            Message = $"Found {mealPlanList.Count} meal plan(s) for {date:yyyy-MM-dd}"
        };
    }
    
    public static AddMealPlanResponse ToAddMealPlanResponse(this MealPlan mealPlan, string message = "Meal plan added successfully")
    {
        ArgumentNullException.ThrowIfNull(mealPlan);
        
        return new AddMealPlanResponse
        {
            Success = true,
            MealPlanId = mealPlan.Id,
            Message = message
        };
    }
}
