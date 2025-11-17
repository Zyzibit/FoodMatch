using inzynierka.Data;
using inzynierka.Receipts.Extensions.Model;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Receipts.Extensions.Repositories;

public class UnitRepository : IUnitRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<UnitRepository> _logger;

    public UnitRepository(AppDbContext context, ILogger<UnitRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Unit> AddUnitAsync(Unit unit)
    {
        try
        {
            _context.Units.Add(unit);
            await _context.SaveChangesAsync();
            return unit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding unit {UnitName}", unit.Name);
            throw;
        }
    }

    public async Task<Unit?> GetUnitByIdAsync(int id)
    {
        try
        {
            return await _context.Units.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit by id {UnitId}", id);
            throw;
        }
    }

    public async Task<Unit?> GetUnitByNameAsync(string name)
    {
        try
        {
            return await _context.Units
                .FirstOrDefaultAsync(u => u.Name.ToLower() == name.ToLower());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit by name {UnitName}", name);
            throw;
        }
    }

    public async Task<List<Unit>> GetAllUnitsAsync()
    {
        try
        {
            return await _context.Units
                .OrderBy(u => u.Name)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all units");
            throw;
        }
    }

    public async Task<Unit?> UpdateUnitAsync(Unit unit)
    {
        try
        {
            var existingUnit = await _context.Units.FindAsync(unit.UnitId);
            if (existingUnit == null)
            {
                return null;
            }

            existingUnit.Name = unit.Name;
            existingUnit.Description = unit.Description;
            existingUnit.PromptDescription = unit.PromptDescription;
            await _context.SaveChangesAsync();
            return existingUnit;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit {UnitId}", unit.UnitId);
            throw;
        }
    }

    public async Task<bool> DeleteUnitAsync(int id)
    {
        try
        {
            var unit = await _context.Units.FindAsync(id);
            if (unit == null)
            {
                return false;
            }

            _context.Units.Remove(unit);
            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit {UnitId}", id);
            throw;
        }
    }

    public async Task<bool> UnitExistsAsync(int id)
    {
        try
        {
            return await _context.Units.AnyAsync(u => u.UnitId == id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if unit exists {UnitId}", id);
            throw;
        }
    }

    public async Task<bool> UnitNameExistsAsync(string name, int? excludeId = null)
    {
        try
        {
            var query = _context.Units.Where(u => u.Name.ToLower() == name.ToLower());
            
            if (excludeId.HasValue)
            {
                query = query.Where(u => u.UnitId != excludeId.Value);
            }
            
            return await query.AnyAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if unit name exists {UnitName}", name);
            throw;
        }
    }
}
