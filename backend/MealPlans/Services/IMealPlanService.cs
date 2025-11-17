using inzynierka.MealPlans.Requests;
using inzynierka.MealPlans.Responses;

namespace inzynierka.MealPlans.Services;

public interface IMealPlanService
{
    public Task<AddMealPlanResponse> AddMealPlanAsync(string userId, CreateMealPlanRequest request);
    public Task<GetMealPlansResponse> GetMealPlansForDateAsync(string userId, DateTime date);
}