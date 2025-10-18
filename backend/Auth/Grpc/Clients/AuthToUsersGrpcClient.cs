using inzynierka.Auth.Contracts.Models;

namespace inzynierka.Auth.Grpc.Clients;

/// <summary>
/// Klient gRPC do komunikacji z serwisem Users z poziomu modułu Auth
/// Auth deleguje operacje na profilu użytkownika do modułu Users
/// </summary>
public class AuthToUsersGrpcClient
{
    private readonly inzynierka.Users.Grpc.UserService.UserServiceClient _client;
    private readonly ILogger<AuthToUsersGrpcClient> _logger;

    public AuthToUsersGrpcClient(
        inzynierka.Users.Grpc.UserService.UserServiceClient client,
        ILogger<AuthToUsersGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }
    
    public async Task<UserInfo?> GetUserInfoAsync(string userId)
    {
        try
        {
            var request = new inzynierka.Users.Grpc.GetUserByIdRequest { UserId = userId };
            var response = await _client.GetUserByIdAsync(request);

            if (!response.Found)
            {
                return null;
            }

            return new UserInfo
            {
                UserId = response.User.Id,
                Username = response.User.Username,
                Email = response.User.Email,
                Roles = new List<string>(), 
                CreatedAt = response.User.CreatedAt.ToDateTime(),
                LastLoginAt = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling Users gRPC service for user {UserId}", userId);
            return null;
        }
    }
    
    public async Task<bool> UpdateUserProfileAsync(string userId, string? email = null, string? name = null)
    {
        try
        {
            var request = new inzynierka.Users.Grpc.UpdateUserProfileRequest
            {
                UserId = userId,
                Email = email ?? string.Empty,
                Name = name ?? string.Empty
            };

            var response = await _client.UpdateUserProfileAsync(request);
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UpdateUserProfile gRPC method for user {UserId}", userId);
            return false;
        }
    }
}
