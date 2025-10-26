using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using inzynierka.Receipts.Contracts;
using inzynierka.Receipts.Contracts.Models;

namespace inzynierka.Receipts.API;

[ApiController]
[Route("api/v1/receipts")]
public class ReceiptsController : ControllerBase
{
    private readonly IRecipeContract _recipeContract;
    private readonly ILogger<ReceiptsController> _logger;

    public ReceiptsController(IRecipeContract recipeContract, ILogger<ReceiptsController> logger)
    {
        _recipeContract = recipeContract;
        _logger = logger;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateReceipt([FromBody] CreateReceiptRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeContract.CreateReceiptAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { success = true, receiptId = result.ReceiptId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating receipt");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetReceipt(int id)
    {
        try
        {
            var receipt = await _recipeContract.GetReceiptAsync(id);
            if (receipt == null)
            {
                return NotFound(new { message = "Receipt not found" });
            }

            return Ok(receipt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting receipt {Id}", id);
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllReceipts([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            var result = await _recipeContract.GetAllReceiptsAsync(limit, offset);
            if (!result.Success)
            {
                return BadRequest(new { message = "Failed to get receipts" });
            }

            return Ok(new {
                receipts = result.Receipts,
                totalCount = result.TotalCount,
                limit,
                offset,
                hasMore = result.TotalCount > (offset + limit)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all receipts");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyReceipts([FromQuery] int limit = 50, [FromQuery] int offset = 0)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeContract.GetUserReceiptsAsync(userId, limit, offset);
            if (!result.Success)
            {
                return BadRequest(new { message = "Failed to get user receipts" });
            }

            return Ok(new {
                receipts = result.Receipts,
                totalCount = result.TotalCount,
                limit,
                offset,
                hasMore = result.TotalCount > (offset + limit)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user receipts");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }

    [HttpPost("generate-with-ai")]
    [Authorize]
    public async Task<IActionResult> GenerateRecipeWithAI([FromBody] GenerateRecipeWithAIRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Invalid token" });
            }

            var result = await _recipeContract.GenerateRecipeWithAIAsync(userId, request);
            if (!result.Success)
            {
                return BadRequest(new { message = result.ErrorMessage });
            }

            return Ok(new { success = true, receiptId = result.ReceiptId, message = "AI recipe generated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recipe with AI");
            return StatusCode(500, new { message = "Internal server error" });
        }
    }
}
