using inzynierka.Receipts.Contracts;
using inzynierka.Receipts.Contracts.Models;
using inzynierka.Receipts.Services;
namespace inzynierka.Receipts.Modules;

public class UnitModule : IUnitContract
{
    private readonly IUnitService _unitService;
    private readonly ILogger<UnitModule> _logger;

    public UnitModule(IUnitService unitService, ILogger<UnitModule> logger)
    {
        _unitService = unitService;
        _logger = logger;
    }

    public async Task<UnitOperationResult> CreateUnitAsync(CreateUnitRequest request)
    {
        try
        {
            return await _unitService.CreateUnitAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UnitModule.CreateUnitAsync");
            return new UnitOperationResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred while creating the unit" 
            };
        }
    }

    public async Task<UnitDto?> GetUnitAsync(int id)
    {
        try
        {
            return await _unitService.GetUnitAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UnitModule.GetUnitAsync");
            return null;
        }
    }

    public async Task<List<UnitDto>> GetAllUnitsAsync()
    {
        try
        {
            return await _unitService.GetAllUnitsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UnitModule.GetAllUnitsAsync");
            return new List<UnitDto>();
        }
    }

    public async Task<UnitOperationResult> UpdateUnitAsync(int id, UpdateUnitRequest request)
    {
        try
        {
            return await _unitService.UpdateUnitAsync(id, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UnitModule.UpdateUnitAsync");
            return new UnitOperationResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred while updating the unit" 
            };
        }
    }

    public async Task<UnitOperationResult> DeleteUnitAsync(int id)
    {
        try
        {
            return await _unitService.DeleteUnitAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in UnitModule.DeleteUnitAsync");
            return new UnitOperationResult 
            { 
                Success = false, 
                ErrorMessage = "An error occurred while deleting the unit" 
            };
        }
    }
}
