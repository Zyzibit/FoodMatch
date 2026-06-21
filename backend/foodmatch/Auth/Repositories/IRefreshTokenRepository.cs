using inzynierka.Auth.Model;

namespace inzynierka.Auth.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken?> GetByUserIdAndDeviceIdAsync(string userId, string deviceId);
    Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(string userId);
    Task<RefreshToken> AddAsync(RefreshToken refreshToken);
    Task UpdateAsync(RefreshToken refreshToken);
    Task RevokeAsync(string token);
    Task RevokeAllByUserIdAsync(string userId);
    Task DeleteExpiredTokensAsync();
}