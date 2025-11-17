using inzynierka.Receipts.Requests;
using inzynierka.Receipts.Responses;

namespace inzynierka.Receipts.Services;

public interface IUnitService
{
    Task<UnitOperationResult> CreateUnitAsync(CreateUnitRequest request);
    Task<UnitDto?> GetUnitAsync(int id);
    Task<List<UnitDto>> GetAllUnitsAsync();
    Task<UnitOperationResult> UpdateUnitAsync(int id, UpdateUnitRequest request);
    Task<UnitOperationResult> DeleteUnitAsync(int id);
}