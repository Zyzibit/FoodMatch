using inzynierka.MealPlans.Constants;
using inzynierka.MealPlans.Extensions;
using inzynierka.MealPlans.Model;
using inzynierka.MealPlans.Repositories;
using inzynierka.MealPlans.Requests;
using inzynierka.MealPlans.Responses;
using inzynierka.Recipes.Services;

namespace inzynierka.MealPlans.Services;

public class MealPlanService: IMealPlanService
{
    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IRecipeService _recipeService;
        
    public MealPlanService(IMealPlanRepository mealPlanRepository, IRecipeService recipeService)
    {
        _mealPlanRepository = mealPlanRepository;
        _recipeService = recipeService;
    }

    public async Task<AddMealPlanResponse> AddMealPlanAsync(string userId, CreateMealPlanRequest request)
    {
        if (!MealNames.IsValidMealName(request.MealName))
            return new AddMealPlanResponse
            {
                Success = false,
                Message = $"Invalid meal name. Allowed values: {string.Join(", ", MealNames.AllowedMealNames)}"
            };

        if (request.RecipeId.HasValue && await _recipeService.GetRecipeAsync(request.RecipeId.Value) == null)
            return new AddMealPlanResponse
            {
                Success = false,
                Message = $"Recipe with ID {request.RecipeId} not found"
            };

        var dateUtc = request.Date.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(request.Date, DateTimeKind.Utc)
            : request.Date.ToUniversalTime();

        var dayStart = dateUtc.Date;
        var dayEnd = dayStart.AddDays(1).AddTicks(-1);
        var plansForDay = await _mealPlanRepository.GetMealPlansForUserAsync(userId, dayStart, dayEnd);
        var existingPlan = plansForDay.FirstOrDefault(p => p.Name == request.MealName);

        if (existingPlan != null)
        {
            existingPlan.RecipeId = request.RecipeId;
            existingPlan.Date = dateUtc;
            await _mealPlanRepository.UpdateMealPlanAsync(existingPlan);
            return existingPlan.ToAddMealPlanResponse("Meal plan updated successfully");
        }

        var mealPlan = new MealPlan
        {
            Name = request.MealName,
            Date = dateUtc,
            RecipeId = request.RecipeId,
            UserId = userId
        };

        await _mealPlanRepository.AddMealPlanAsync(mealPlan);
        return mealPlan.ToAddMealPlanResponse();
    }

    public async Task<GetMealPlansForDateResponse> GetMealPlansForDateAsync(string userId, DateTime date)
    {
        var dateUtc = date.Kind == DateTimeKind.Unspecified 
            ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
            : date.ToUniversalTime();

        var startOfDay = dateUtc.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

        var mealPlans = await _mealPlanRepository.GetMealPlansForUserAsync(userId, startOfDay, endOfDay);
        var mealPlanDtos = mealPlans.Select(mp => mp.ToDto()).ToList();

        return mealPlanDtos.ToGetMealPlansForDateResponse(date);
    }

    public async Task<bool> DeleteMealPlanAsync(string userId, int mealPlanId)
    {
        var mealPlan = await _mealPlanRepository.GetMealPlanAsync(mealPlanId);
        
        if (mealPlan == null || mealPlan.UserId != userId)
            return false;

        await _mealPlanRepository.DeleteMealPlanAsync(mealPlanId);
        return true;
    }
}