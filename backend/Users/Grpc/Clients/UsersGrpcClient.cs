using inzynierka.Users.Contracts.Models;

namespace inzynierka.Users.Grpc.Clients;

public class UsersGrpcClient
{
    private readonly inzynierka.Users.Grpc.UserService.UserServiceClient _client;
    private readonly ILogger<UsersGrpcClient> _logger;

    public UsersGrpcClient(
        inzynierka.Users.Grpc.UserService.UserServiceClient client,
        ILogger<UsersGrpcClient> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        try
        {
            var request = new GetUserByIdRequest { UserId = userId };
            var response = await _client.GetUserByIdAsync(request);

            if (!response.Found)
            {
                return null;
            }

            return MapToUserDto(response.User);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GetUserById gRPC method for user {UserId}", userId);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        try
        {
            var request = new GetUserByUsernameRequest { Username = username };
            var response = await _client.GetUserByUsernameAsync(request);

            if (!response.Found)
            {
                return null;
            }

            return MapToUserDto(response.User);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GetUserByUsername gRPC method for username {Username}", username);
            throw;
        }
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        try
        {
            var request = new GetUserByEmailRequest { Email = email };
            var response = await _client.GetUserByEmailAsync(request);

            if (!response.Found)
            {
                return null;
            }

            return MapToUserDto(response.User);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling GetUserByEmail gRPC method for email {Email}", email);
            throw;
        }
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, string? name, string? email)
    {
        try
        {
            var request = new UpdateUserProfileRequest
            {
                UserId = userId,
                Name = name ?? string.Empty,
                Email = email ?? string.Empty
            };

            var response = await _client.UpdateUserProfileAsync(request);
            return response.Success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling UpdateUserProfile gRPC method for user {UserId}", userId);
            throw;
        }
    }

    private static UserDto MapToUserDto(inzynierka.Users.Grpc.UserDto grpcUser)
    {
        return new UserDto
        {
            Id = grpcUser.Id,
            Name = grpcUser.Name,
            Email = grpcUser.Email,
            CreatedAt = grpcUser.CreatedAt,
            UpdatedAt = grpcUser.UpdatedAt != default ? grpcUser.UpdatedAt : null 
        };
    }
}

