using Microsoft.EntityFrameworkCore;
using inzynierka.Data;
using inzynierka.Receipts.Model;

namespace inzynierka.Receipts.Repositories;

public class ReceiptRepository : IReceiptRepository
{
    private readonly AppDbContext _db;

    public ReceiptRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<Receipt> AddReceiptAsync(Receipt receipt)
    {
        _db.Receipts.Add(receipt);
        await _db.SaveChangesAsync();
        return receipt;
    }

    public async Task<Receipt?> GetReceiptByIdAsync(int id)
    {
        return await _db.Receipts
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Unit)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<(List<Receipt> Receipts, int TotalCount)> GetAllReceiptsAsync(int limit = 50, int offset = 0)
    {
        var total = await _db.Receipts.CountAsync();
        var receipts = await _db.Receipts
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Unit)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return (receipts, total);
    }

    public async Task<(List<Receipt> Receipts, int TotalCount)> GetUserReceiptsAsync(string userId, int limit = 50, int offset = 0)
    {
        var query = _db.Receipts.Where(r => r.UserId == userId);
        var total = await query.CountAsync();
        var receipts = await query
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Unit)
            .Include(r => r.Ingredients)
                .ThenInclude(ri => ri.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Skip(offset)
            .Take(limit)
            .ToListAsync();

        return (receipts, total);
    }
}
