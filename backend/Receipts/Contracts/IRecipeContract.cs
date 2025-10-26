using inzynierka.Receipts.Contracts.Models;

namespace inzynierka.Receipts.Contracts;

public interface IRecipeContract
{
    Task<CreateReceiptResult> CreateReceiptAsync(string userId, CreateReceiptRequest request);
    Task<ReceiptDto?> GetReceiptAsync(int id);
    Task<ReceiptsListResult> GetAllReceiptsAsync(int limit = 50, int offset = 0);
    Task<ReceiptsListResult> GetUserReceiptsAsync(string userId, int limit = 50, int offset = 0);
    Task<CreateReceiptResult> GenerateRecipeWithAIAsync(string userId, GenerateRecipeWithAIRequest request);
}
