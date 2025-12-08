using inzynierka.Data;
using inzynierka.MealPlans.Model;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.MealPlans.Repositories;

public class MealPlanRepository : IMealPlanRepository
{
    private readonly AppDbContext _dbContext;

    public MealPlanRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task AddMealPlanAsync(MealPlan mealPlan)
    {
        try
        {
            _dbContext.MealPlans.Add(mealPlan);
            return _dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            throw new Exception("Error adding meal plan", ex);
        }
    }

    public async Task<MealPlan?> GetMealPlanAsync(int mealPlanId)
    {
        return await _dbContext.MealPlans
            .Include(mp => mp.Recipe)
            .FirstOrDefaultAsync(mp => mp.Id == mealPlanId);
    }

    public async Task<MealPlan?> GetMealPlanForDateAndName(DateTime date, string userId, string name)
    {
        return await _dbContext.MealPlans
            .FirstOrDefaultAsync(mp => 
                mp.UserId == userId && 
                mp.Name == name && 
                mp.Date == date);
    }

    public async Task<List<MealPlan>> GetMealPlansForUserAsync(string userId, DateTime startDate, DateTime endDate)
    {
        return await _dbContext.MealPlans
            .Include(mp => mp.Recipe)
                .ThenInclude(r => r!.Ingredients)
                    .ThenInclude(i => i.Product)
            .Include(mp => mp.Recipe)
                .ThenInclude(r => r!.Ingredients)
                    .ThenInclude(i => i.Unit)
            .Where(mp => mp.UserId == userId && mp.Date >= startDate && mp.Date <= endDate)
            .OrderBy(mp => mp.Date)
            .ToListAsync();
    }

    public async Task DeleteMealPlanAsync(int mealPlanId)
    {
        var mealPlan = await _dbContext.MealPlans.FindAsync(mealPlanId);
        if (mealPlan != null)
        {
            _dbContext.MealPlans.Remove(mealPlan);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateMealPlanAsync(MealPlan mealPlan)
    {
        _dbContext.MealPlans.Update(mealPlan);
        await _dbContext.SaveChangesAsync();
    }
}