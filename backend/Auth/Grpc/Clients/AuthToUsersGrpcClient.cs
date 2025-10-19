using Microsoft.Extensions.Logging;

namespace inzynierka.Auth.Grpc.Clients;

/// <summary>
/// Stub replacement for previous Auth->Users gRPC client.
/// Auth should not call Users directly; use EventBus-based events instead.
/// This class intentionally throws if used to surface design violation.
/// </summary>
public class AuthToUsersGrpcClient
{
    private readonly ILogger<AuthToUsersGrpcClient> _logger;

    public AuthToUsersGrpcClient(ILogger<AuthToUsersGrpcClient> logger)
    {
        _logger = logger;
    }

    public Task<object?> GetUserInfoAsync(string userId)
    {
        _logger.LogError("AuthToUsersGrpcClient was called directly. Replace direct calls with EventBus communication.");
        throw new NotSupportedException("Direct calls from Auth to Users are not supported. Use EventBus events.");
    }

    public Task<bool> UpdateUserProfileAsync(string userId, string? email = null, string? name = null)
    {
        _logger.LogError("AuthToUsersGrpcClient.UpdateUserProfileAsync was called. Replace with EventBus.");
        throw new NotSupportedException("Direct calls from Auth to Users are not supported. Use EventBus events.");
    }
}
