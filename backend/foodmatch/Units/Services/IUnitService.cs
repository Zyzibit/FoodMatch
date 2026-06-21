using foodmatch.Units.Responses;
using foodmatch.Units.Requests;
using foodmatch.Units.Responses;

namespace foodmatch.Units.Services;

public interface IUnitService
{
    Task<UnitOperationResult> CreateUnitAsync(CreateUnitRequest request);
    Task<UnitDto?> GetUnitAsync(int id);
    Task<UnitDto?> GetUnitByNameAsync(string name);
    Task<List<UnitDto>> GetAllUnitsAsync();
    Task<UnitOperationResult> UpdateUnitAsync(int id, UpdateUnitRequest request);
    Task<UnitOperationResult> DeleteUnitAsync(int id);
}