using inzynierka.MealPlans.Model;

namespace inzynierka.MealPlans.Repositories;

public interface IMealPlanRepository
{
    public Task AddMealPlanAsync(MealPlan mealPlan);
    public Task<MealPlan?> GetMealPlanAsync(int mealPlanId);
    public Task<MealPlan?> GetMealPlanForDateAndName(DateTime date, string userId, string name);
    public Task<List<MealPlan>> GetMealPlansForUserAsync(string userId, DateTime startDate, DateTime endDate);
    public Task DeleteMealPlanAsync(int mealPlanId);
    public Task UpdateMealPlanAsync(MealPlan mealPlan);
}