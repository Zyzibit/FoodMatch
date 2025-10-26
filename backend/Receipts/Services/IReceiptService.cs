using inzynierka.Receipts.Contracts.Models;
using inzynierka.Receipts.Model;

namespace inzynierka.Receipts.Services;

public interface IReceiptService
{
    Task<CreateReceiptResult> CreateReceiptAsync(string userId, CreateReceiptRequest receipt);
    Task<ReceiptDto?> GetReceiptAsync(int id);
    Task<ReceiptsListResult> GetAllReceiptsAsync(int limit = 50, int offset = 0);
    Task<ReceiptsListResult> GetUserReceiptsAsync(string userId, int limit = 50, int offset = 0);
    Task<CreateReceiptResult> GenerateRecipeWithAiAsync(string userId, GenerateRecipeWithAIRequest request);
}
