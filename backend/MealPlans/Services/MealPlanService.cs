using inzynierka.MealPlans.Constants;
using inzynierka.MealPlans.Model;
using inzynierka.MealPlans.Repositories;
using inzynierka.MealPlans.Requests;
using inzynierka.MealPlans.Responses;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.MealPlans.Services;

public class MealPlanService: IMealPlanService
{
    private readonly IMealPlanRepository _mealPlanRepository;
        
    public MealPlanService(IMealPlanRepository mealPlanRepository)
    {
        _mealPlanRepository = mealPlanRepository;
    }

    public async Task<AddMealPlanResponse> AddMealPlanAsync(string userId, CreateMealPlanRequest request)
    {
        try
        {
            if (!MealPlanConstants.IsValidMealName(request.Name))
            {
                return new AddMealPlanResponse
                {
                    Success = false,
                    Message = $"Invalid meal name. Allowed values: {string.Join(", ", MealPlanConstants.AllowedMealNames)}"
                };
            }
            
            var dateUtc = request.Date.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(request.Date, DateTimeKind.Utc)
                : request.Date.ToUniversalTime();
            
            var dayStart = dateUtc.Date;
            var dayEnd = dayStart.AddDays(1).AddTicks(-1);
            var plansForDay = await _mealPlanRepository.GetMealPlansForUserAsync(userId, dayStart, dayEnd);
            
            var existingPlan = plansForDay.FirstOrDefault(p => p.Name == request.Name);
            
            if (existingPlan != null)
            {
                existingPlan.ReceiptId = request.ReceiptId;
                existingPlan.Date = dateUtc;
                await _mealPlanRepository.UpdateMealPlanAsync(existingPlan);
                
                return new AddMealPlanResponse
                {
                    Success = true,
                    MealPlanId = existingPlan.Id,
                    Message = "Meal plan updated successfully"
                };
            }
            
            var mealPlan = new MealPlan
            {
                Name = request.Name,
                Date = dateUtc,
                ReceiptId = request.ReceiptId,
                UserId = userId
            };
            
            await _mealPlanRepository.AddMealPlanAsync(mealPlan);
            
            return new AddMealPlanResponse
            {
                Success = true,
                MealPlanId = mealPlan.Id,
                Message = "Meal plan added successfully"
            };
        }
        catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("FK_MealPlans_Receipts_ReceiptId") == true)
        {
            return new AddMealPlanResponse
            {
                Success = false,
                Message = $"Receipt with ID {request.ReceiptId} not found"
            };
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

    public async Task<GetMealPlansResponse> GetMealPlansForDateAsync(string userId, DateTime date)
    {
        try
        {
            var dateUtc = date.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(date, DateTimeKind.Utc)
                : date.ToUniversalTime();
            
            var startOfDay = dateUtc.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
            
            var mealPlans = await _mealPlanRepository.GetMealPlansForUserAsync(userId, startOfDay, endOfDay);
            
            var mealPlanDtos = mealPlans.Select(mp => new MealPlanDto
            {
                Id = mp.Id,
                Name = mp.Name,
                Date = mp.Date,
                Receipt = mp.Receipt != null ? new MealPlanReceiptDto
                {
                    Id = mp.Receipt.Id,
                    Title = mp.Receipt.Title,
                    Description = mp.Receipt.Description,
                    CaloriesPer100G = mp.Receipt.Calories,
                    ProteinPer100G = mp.Receipt.Protein,
                    CarbohydratesPer100G = mp.Receipt.Carbohydrates,
                    FatsPer100G = mp.Receipt.Fats,
                    Servings = mp.Receipt.Servings,
                    PreparationTimeMinutes = mp.Receipt.PreparationTimeMinutes
                } : null
            }).ToList();
            
            return new GetMealPlansResponse
            {
                Success = true,
                MealPlans = mealPlanDtos,
                Message = $"Found {mealPlanDtos.Count} meal plan(s) for {date:yyyy-MM-dd}"
            };
        }
        catch (Exception ex)
        {
            return new GetMealPlansResponse
            {
                Success = false,
                MealPlans = new List<MealPlanDto>(),
                Message = $"Error getting meal plans: {ex.Message}"
            };
        }
    }
}