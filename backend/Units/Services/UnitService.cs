using inzynierka.Units.Dto;
using inzynierka.Units.Models;
using inzynierka.Units.Repositories;
using inzynierka.Units.Requests;
using inzynierka.Units.Extensions;
using inzynierka.Units.Responses;

namespace inzynierka.Units.Services;

public class UnitService : IUnitService
{
    private readonly IUnitRepository _unitRepository;
    private readonly ILogger<UnitService> _logger;

    public UnitService(IUnitRepository unitRepository, ILogger<UnitService> logger)
    {
        _unitRepository = unitRepository;
        _logger = logger;
    }

    public async Task<UnitOperationResult> CreateUnitAsync(CreateUnitRequest request)
    {
        try
        {
            if (await _unitRepository.UnitNameExistsAsync(request.Name))
            {
                return new UnitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Unit with name '{request.Name}' already exists"
                };
            }

            var unit = new Unit
            {
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                PromptDescription = request.PromptDescription.Trim()
            };

            var createdUnit = await _unitRepository.AddUnitAsync(unit);

            return new UnitOperationResult
            {
                Success = true,
                Unit = createdUnit.ToDto()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit {UnitName}", request.Name);
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
            var unit = await _unitRepository.GetUnitByIdAsync(id);
            return unit != null ? unit.ToDto() : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit {UnitId}", id);
            return null;
        }
    }

    public async Task<List<UnitDto>> GetAllUnitsAsync()
    {
        try
        {
            var units = await _unitRepository.GetAllUnitsAsync();
            return units.ToDtoList().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all units");
            return new List<UnitDto>();
        }
    }

    public async Task<UnitOperationResult> UpdateUnitAsync(int id, UpdateUnitRequest request)
    {
        try
        {
            if (!await _unitRepository.UnitExistsAsync(id))
            {
                return new UnitOperationResult
                {
                    Success = false,
                    ErrorMessage = "Unit not found"
                };
            }

            if (await _unitRepository.UnitNameExistsAsync(request.Name, id))
            {
                return new UnitOperationResult
                {
                    Success = false,
                    ErrorMessage = $"Unit with name '{request.Name}' already exists"
                };
            }

            var unit = new Unit
            {
                UnitId = id,
                Name = request.Name.Trim(),
                Description = request.Description.Trim(),
                PromptDescription = request.PromptDescription.Trim()
            };

            var updatedUnit = await _unitRepository.UpdateUnitAsync(unit);

            if (updatedUnit == null)
            {
                return new UnitOperationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to update unit"
                };
            }

            return new UnitOperationResult
            {
                Success = true,
                Unit = updatedUnit.ToDto()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit {UnitId}", id);
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
            if (!await _unitRepository.UnitExistsAsync(id))
            {
                return new UnitOperationResult
                {
                    Success = false,
                    ErrorMessage = "Unit not found"
                };
            }

            var deleted = await _unitRepository.DeleteUnitAsync(id);

            if (!deleted)
            {
                return new UnitOperationResult
                {
                    Success = false,
                    ErrorMessage = "Failed to delete unit"
                };
            }

            return new UnitOperationResult
            {
                Success = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit {UnitId}", id);
            return new UnitOperationResult
            {
                Success = false,
                ErrorMessage = "An error occurred while deleting the unit. Make sure the unit is not used in any recipes."
            };
        }
    }
}
