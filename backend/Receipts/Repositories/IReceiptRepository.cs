using inzynierka.Receipts.Model;

namespace inzynierka.Receipts.Repositories;

public interface IReceiptRepository
{
    Task<Receipt> AddReceiptAsync(Receipt receipt);
    Task<Receipt?> GetReceiptByIdAsync(int id);
    Task<(List<Receipt> Receipts, int TotalCount)> GetAllReceiptsAsync(int limit = 50, int offset = 0);
    Task<(List<Receipt> Receipts, int TotalCount)> GetUserReceiptsAsync(string userId, int limit = 50, int offset = 0);
}
