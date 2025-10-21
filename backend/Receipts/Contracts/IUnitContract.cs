using inzynierka.Receipts.Contracts.Models;

namespace inzynierka.Receipts.Contracts;

public interface IUnitContract
{
    Task<UnitOperationResult> CreateUnitAsync(CreateUnitRequest request);
    Task<UnitDto?> GetUnitAsync(int id);
    Task<List<UnitDto>> GetAllUnitsAsync();
    Task<UnitOperationResult> UpdateUnitAsync(int id, UpdateUnitRequest request);
    Task<UnitOperationResult> DeleteUnitAsync(int id);
}

