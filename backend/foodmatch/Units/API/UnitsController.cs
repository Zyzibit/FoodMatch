using inzynierka.Units.Requests;
using inzynierka.Units.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace inzynierka.Units.API;

[ApiController]
[Route("api/v1/units")]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _unitService;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(IUnitService unitService, ILogger<UnitsController> logger)
    {
        _unitService = unitService;
        _logger = logger;
    }


    [HttpGet]
    public async Task<IActionResult> GetAllUnits() {
        try {
            var units = await _unitService.GetAllUnitsAsync();
            return Ok(units);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error getting all units");
            return StatusCode(500, new { message = "An error occurred while getting units" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetUnit(int id)
    {
        try
        {
            var unit = await _unitService.GetUnitAsync(id);
            if (unit == null)
            {
                return NotFound(new { message = "Unit not found" });
            }

            return Ok(unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unit {UnitId}", id);
            return StatusCode(500, new { message = "An error occurred while getting the unit" });
        }
    }


    [HttpPost]
    [Authorize (Roles = "Admin")]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request)
    {
        try
        {
            var result = await _unitService.CreateUnitAsync(request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return CreatedAtAction(nameof(GetUnit), new { id = result.Unit?.UnitId }, result.Unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit");
            return StatusCode(500, new { message = "An error occurred while creating the unit" });
        }
    }
    
    [HttpPut("{id}")]
    [Authorize (Roles = "Admin")]
    public async Task<IActionResult> UpdateUnit(int id, [FromBody] UpdateUnitRequest request)
    {
        try
        {
            var result = await _unitService.UpdateUnitAsync(id, request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(result.Unit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating unit {UnitId}", id);
            return StatusCode(500, new { message = "An error occurred while updating the unit" });
        }
    }

    [HttpDelete("{id}")]
    [Authorize (Roles = "Admin")]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        try
        {
            var result = await _unitService.DeleteUnitAsync(id);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting unit {UnitId}", id);
            return StatusCode(500, new { message = "An error occurred while deleting the unit" });
        }
    }
}
