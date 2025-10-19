using inzynierka.Auth.Model;
using inzynierka.Data;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Auth.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly AppDbContext _context;

    public RefreshTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    public async Task<RefreshToken?> GetByUserIdAndDeviceIdAsync(string userId, string deviceId)
    {
        var currentTime = DateTime.UtcNow;
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.UserId == userId && 
                                      rt.DeviceId == deviceId && 
                                      rt.RevokedAt == null &&
                                      rt.ExpiryDate > currentTime);
    }

    public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId)
    {
        var currentTime = DateTime.UtcNow;
        return await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && 
                        rt.RevokedAt == null && 
                        rt.ExpiryDate > currentTime)
            .ToListAsync();
    }

    public async Task<RefreshToken> AddAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Add(refreshToken);
        await _context.SaveChangesAsync();
        return refreshToken;
    }

    public async Task UpdateAsync(RefreshToken refreshToken)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync();
    }

    public async Task RevokeAsync(string token)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken != null && 
            refreshToken.RevokedAt == null && 
            refreshToken.ExpiryDate > DateTime.UtcNow)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
            await UpdateAsync(refreshToken);
        }
    }

    public async Task RevokeAllByUserIdAsync(string userId)
    {
        var currentTime = DateTime.UtcNow;
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && 
                        rt.RevokedAt == null && 
                        rt.ExpiryDate > currentTime)
            .ToListAsync();
            
        foreach (var token in tokens)
        {
            token.RevokedAt = DateTime.UtcNow;
        }
        await _context.SaveChangesAsync();
    }

    public async Task DeleteExpiredTokensAsync()
    {
        var expiredTokens = await _context.RefreshTokens
            .Where(rt => rt.ExpiryDate < DateTime.UtcNow || rt.RevokedAt != null)
            .ToListAsync();
            
        _context.RefreshTokens.RemoveRange(expiredTokens);
        await _context.SaveChangesAsync();
    }
}