using inzynierka.MealPlans.Constants;
using inzynierka.MealPlans.Extensions;
using inzynierka.MealPlans.Model;
using inzynierka.MealPlans.Repositories;
using inzynierka.MealPlans.Requests;
using inzynierka.MealPlans.Responses;
using inzynierka.Recipes.Services;
using Microsoft.Extensions.Logging;

namespace inzynierka.MealPlans.Services;

public class MealPlanService: IMealPlanService
{
    private readonly IMealPlanRepository _mealPlanRepository;
    private readonly IRecipeService _recipeService;
    private readonly ILogger<MealPlanService> _logger;
        
    public MealPlanService(IMealPlanRepository mealPlanRepository, IRecipeService recipeService, ILogger<MealPlanService> logger)
    {
        _mealPlanRepository = mealPlanRepository;
        _recipeService = recipeService;
        _logger = logger;
    }

    public async Task<AddMealPlanResponse> AddMealPlanAsync(string userId, CreateMealPlanRequest request)
    {
        try
        {
            if (!MealNames.IsValidMealName(request.MealName))
            {
                return new AddMealPlanResponse
                {
                    Success = false,
                    Message = $"Invalid meal name. Allowed values: {string.Join(", ", MealNames.AllowedMealNames)}"
                };
            }
            
            if (request.RecipeId.HasValue)
            {
                var recipe = await _recipeService.GetRecipeAsync(request.RecipeId.Value);
                if (recipe == null)
                {
                    return new AddMealPlanResponse
                    {
                        Success = false,
                        Message = $"Recipe with ID {request.RecipeId} not found"
                    };
                }
            }
            
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
        catch (Exception ex)
        {
            return new AddMealPlanResponse
            {
                Success = false,
                Message = $"Error adding meal plan: {ex.Message}"
            };
        }
    }

    public async Task<GetMealPlansForDateResponse> GetMealPlansForDateAsync(string userId, DateTime date)
    {
        try
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
        catch (Exception ex)
        {
            return new GetMealPlansForDateResponse
            {
                Success = false,
                MealPlans = new List<MealPlanDto>(),
                Message = $"Error getting meal plans: {ex.Message}"
            };
        }
    }

    public async Task<bool> DeleteMealPlanAsync(string userId, int mealPlanId)
    {
        try
        {
            _logger.LogInformation("Attempting to delete meal plan {MealPlanId} for user {UserId}", mealPlanId, userId);
            
            var mealPlan = await _mealPlanRepository.GetMealPlanAsync(mealPlanId);
            
            if (mealPlan == null)
            {
                _logger.LogWarning("Meal plan with ID {MealPlanId} not found", mealPlanId);
                return false;
            }

            _logger.LogInformation("Found meal plan {MealPlanId} owned by user {OwnerId}", mealPlanId, mealPlan.UserId);

            if (mealPlan.UserId != userId)
            {
                _logger.LogWarning("User {UserId} attempted to delete meal plan {MealPlanId} owned by {OwnerId}", 
                    userId, mealPlanId, mealPlan.UserId);
                return false;
            }

            await _mealPlanRepository.DeleteMealPlanAsync(mealPlanId);
            _logger.LogInformation("Meal plan {MealPlanId} deleted successfully by user {UserId}", mealPlanId, userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting meal plan {MealPlanId}", mealPlanId);
            return false;
        }
    }
}