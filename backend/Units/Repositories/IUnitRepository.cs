using inzynierka.Units.Models;

namespace inzynierka.Units.Repositories;

public interface IUnitRepository
{
    Task<Unit> AddUnitAsync(Unit unit);
    Task<Unit?> GetUnitByIdAsync(int id);
    Task<Unit?> GetUnitByNameAsync(string name);
    Task<List<Unit>> GetAllUnitsAsync();
    Task<Unit?> UpdateUnitAsync(Unit unit);
    Task<bool> DeleteUnitAsync(int id);
    Task<bool> UnitExistsAsync(int id);
    Task<bool> UnitNameExistsAsync(string name, int? excludeId = null);
}

