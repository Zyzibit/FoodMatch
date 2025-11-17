using inzynierka.Receipts.Extensions.Requests;
using inzynierka.Receipts.Extensions.Responses;

namespace inzynierka.Receipts.Extensions.Services;

public interface IUnitService
{
    Task<UnitOperationResult> CreateUnitAsync(CreateUnitRequest request);
    Task<UnitDto?> GetUnitAsync(int id);
    Task<List<UnitDto>> GetAllUnitsAsync();
    Task<UnitOperationResult> UpdateUnitAsync(int id, UpdateUnitRequest request);
    Task<UnitOperationResult> DeleteUnitAsync(int id);
}