using inzynierka.Data;
using inzynierka.Recipes.Model;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Recipes.Repositories;

public class RecipeRepository : IRecipeRepository
{
    private readonly AppDbContext _db;

    public RecipeRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Recipe> AddRecipeAsync(Recipe recipe)
    {
        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync();
        return recipe;
    }

    public async Task<Recipe?> GetRecipeByIdAsync(int id)
    {
        return await _db.Recipes
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Unit)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<Recipe> UpdateRecipeAsync(Recipe recipe)
    {
        _db.Recipes.Update(recipe);
        await _db.SaveChangesAsync();
        return recipe;
    }

    public async Task DeleteRecipeAsync(int recipeId)
    {
        var recipe = await _db.Recipes.FindAsync(recipeId);
        if (recipe != null)
        {
            _db.Recipes.Remove(recipe);
            await _db.SaveChangesAsync();
        }
    }

    public async Task<(List<Recipe> Recipes, int TotalCount)> GetAllRecipesAsync(int limit = 50, int offset = 0)
    {
        var total = await _db.Recipes.CountAsync();
        var recipes = await _db.Recipes
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Unit)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return (recipes, total);
    }

    public async Task<(List<Recipe> Recipes, int TotalCount)> GetUserRecipesAsync(string userId, int limit = 50, int offset = 0)
    {
        var query = _db.Recipes.Where(r => r.UserId == userId);
        var total = await query.CountAsync();
        var recipes = await query
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Unit)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return (recipes, total);
    }

    public async Task<(List<Recipe> Recipes, int TotalCount)> GetPublicRecipesAsync(int limit = 50, int offset = 0)
    {
        var query = _db.Recipes.Where(r => r.IsPublic);
        var total = await query.CountAsync();
        var recipes = await query
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Unit)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return (recipes, total);
    }

    public async Task<Recipe?> GetRecipeByIdForCopyAsync(int id)
    {
        return await _db.Recipes
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Unit)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .FirstOrDefaultAsync(r => r.Id == id && r.IsPublic);
    }

    public async Task<(List<Recipe> Recipes, int TotalCount)> SearchRecipesAsync(
        string? searchTerm, 
        bool isPublicOnly, 
        string? userId, 
        int limit = 50, 
        int offset = 0)
    {
        var query = _db.Recipes.AsQueryable();

        // Filter by public/user
        if (isPublicOnly || string.IsNullOrEmpty(userId))
        {
            query = query.Where(r => r.IsPublic);
        }
        else
        {
            query = query.Where(r => r.UserId == userId || r.IsPublic);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var lowerSearchTerm = searchTerm.ToLower();
            query = query.Where(r => r.Title.ToLower().Contains(lowerSearchTerm));
        }

        var total = await query.CountAsync();
        var recipes = await query
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Unit)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return (recipes, total);
    }
}
