using inzynierka.MealPlans.Requests;
using inzynierka.MealPlans.Responses;

namespace inzynierka.MealPlans.Services;

public interface IMealPlanService
{
    public Task<AddMealPlanResponse> AddMealPlanAsync(string userId, CreateMealPlanRequest request);
    public Task<GetMealPlansForDateResponse> GetMealPlansForDateAsync(string userId, DateTime date);
    public Task<bool> DeleteMealPlanAsync(string userId, int mealPlanId);
}