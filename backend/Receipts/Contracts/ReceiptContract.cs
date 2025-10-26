using inzynierka.Receipts.Contracts;
using inzynierka.Receipts.Contracts.Models;
using inzynierka.Receipts.Model;
using inzynierka.Receipts.Services;

namespace inzynierka.Receipts.Modules;

public class ReceiptContract : IRecipeContract
{
    private readonly IReceiptService _receiptService;
    private readonly ILogger<ReceiptContract> _logger;

    public ReceiptContract(IReceiptService receiptService, ILogger<ReceiptContract> logger)
    {
        _receiptService = receiptService;
        _logger = logger;
    }

    public async Task<CreateReceiptResult> CreateReceiptAsync(string userId, CreateReceiptRequest request)
    {
        try {
            return await _receiptService.CreateReceiptAsync(userId, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReceiptContract.CreateReceiptAsync");
            return new CreateReceiptResult { Success = false, ErrorMessage = ex.Message };
        }
    }

    public Task<ReceiptDto?> GetReceiptAsync(int id)
    {
        try
        {
            return _receiptService.GetReceiptAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReceiptContract.GetReceiptAsync");
            return Task.FromResult<ReceiptDto?>(null);
        }
    }

    public Task<ReceiptsListResult> GetAllReceiptsAsync(int limit = 50, int offset = 0)
    {
        try
        {
            return _receiptService.GetAllReceiptsAsync(limit, offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReceiptContract.GetAllReceiptsAsync");
            return Task.FromResult(new ReceiptsListResult { Success = false, Receipts = new List<ReceiptDto>(), TotalCount = 0 });
        }
    }

    public Task<ReceiptsListResult> GetUserReceiptsAsync(string userId, int limit = 50, int offset = 0)
    {
        try
        {
            return _receiptService.GetUserReceiptsAsync(userId, limit, offset);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReceiptContract.GetUserReceiptsAsync");
            return Task.FromResult(new ReceiptsListResult { Success = false, Receipts = new List<ReceiptDto>(), TotalCount = 0 });
        }
    }

    public async Task<CreateReceiptResult> GenerateRecipeWithAIAsync(string userId, GenerateRecipeWithAIRequest request)
    {
        try
        {
            return await _receiptService.GenerateRecipeWithAiAsync(userId, request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in ReceiptContract.GenerateRecipeWithAIAsync");
            return new CreateReceiptResult { Success = false, ErrorMessage = ex.Message };
        }
    }
}
