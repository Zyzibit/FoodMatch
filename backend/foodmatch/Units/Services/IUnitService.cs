using inzynierka.Units.Dto;
using inzynierka.Units.Requests;
using inzynierka.Units.Responses;

namespace inzynierka.Units.Services;

public interface IUnitService
{
    Task<UnitOperationResult> CreateUnitAsync(CreateUnitRequest request);
    Task<UnitDto?> GetUnitAsync(int id);
    Task<UnitDto?> GetUnitByNameAsync(string name);
    Task<List<UnitDto>> GetAllUnitsAsync();
    Task<UnitOperationResult> UpdateUnitAsync(int id, UpdateUnitRequest request);
    Task<UnitOperationResult> DeleteUnitAsync(int id);
}