using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using inzynierka.Receipts.Contracts;
using inzynierka.Receipts.Contracts.Models;

namespace inzynierka.Receipts.API;

[ApiController]
[Route("api/v1/units")]
public class UnitsController : ControllerBase
{
    private readonly IUnitContract _unitContract;
    private readonly ILogger<UnitsController> _logger;

    public UnitsController(IUnitContract unitContract, ILogger<UnitsController> logger)
    {
        _unitContract = unitContract;
        _logger = logger;
    }

    /// <summary>
    /// Gets all units of measure
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllUnits()
    {
        try
        {
            var units = await _unitContract.GetAllUnitsAsync();
            return Ok(units);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all units");
            return StatusCode(500, new { message = "An error occurred while getting units" });
        }
    }

    /// <summary>
    /// Gets a unit by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUnit(int id)
    {
        try
        {
            var unit = await _unitContract.GetUnitAsync(id);
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
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> CreateUnit([FromBody] CreateUnitRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _unitContract.CreateUnitAsync(request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return CreatedAtAction(
                nameof(GetUnit),
                new { id = result.Unit!.UnitId },
                result.Unit
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating unit");
            return StatusCode(500, new { message = "An error occurred while creating the unit" });
        }
    }

    /// <summary>
    /// Updates an existing unit of measure
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> UpdateUnit(int id, [FromBody] UpdateUnitRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _unitContract.UpdateUnitAsync(id, request);
            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(new { message = result.ErrorMessage });
                }
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

    /// <summary>
    /// Deletes a unit of measure
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteUnit(int id)
    {
        try
        {
            var result = await _unitContract.DeleteUnitAsync(id);
            if (!result.Success)
            {
                if (result.ErrorMessage?.Contains("not found") == true)
                {
                    return NotFound(new { message = result.ErrorMessage });
                }
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
