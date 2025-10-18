using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using inzynierka.Users.Contracts;

namespace inzynierka.Users.Grpc.Services;

public class UsersGrpcService : UserService.UserServiceBase
{
    private readonly IUsersContract _usersContract;
    private readonly ILogger<UsersGrpcService> _logger;

    public UsersGrpcService(IUsersContract usersContract, ILogger<UsersGrpcService> logger)
    {
        _usersContract = usersContract;
        _logger = logger;
    }

    public override async Task<GetUserByIdResponse> GetUserById(GetUserByIdRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _usersContract.GetUserByIdAsync(request.UserId);
            
            if (user == null)
            {
                return new GetUserByIdResponse { Found = false };
            }

            return new GetUserByIdResponse
            {
                Found = true,
                User = MapToGrpcUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving user"));
        }
    }

    public override async Task<GetUserByUsernameResponse> GetUserByUsername(GetUserByUsernameRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _usersContract.GetUserByUsernameAsync(request.Username);
            
            if (user == null)
            {
                return new GetUserByUsernameResponse { Found = false };
            }

            return new GetUserByUsernameResponse
            {
                Found = true,
                User = MapToGrpcUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username: {Username}", request.Username);
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving user"));
        }
    }

    public override async Task<GetUserByEmailResponse> GetUserByEmail(GetUserByEmailRequest request, ServerCallContext context)
    {
        try
        {
            var user = await _usersContract.GetUserByEmailAsync(request.Email);
            
            if (user == null)
            {
                return new GetUserByEmailResponse { Found = false };
            }

            return new GetUserByEmailResponse
            {
                Found = true,
                User = MapToGrpcUserDto(user)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", request.Email);
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving user"));
        }
    }

    public override async Task<GetUsersResponse> GetUsers(GetUsersRequest request, ServerCallContext context)
    {
        try
        {
            var users = await _usersContract.GetUsersAsync(request.PageNumber, request.PageSize);
            var totalCount = await _usersContract.GetTotalUsersCountAsync();

            var response = new GetUsersResponse
            {
                TotalCount = totalCount,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize)
            };

            response.Users.AddRange(users.Select(MapToGrpcUserDto));

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users");
            throw new RpcException(new Status(StatusCode.Internal, "Error retrieving users"));
        }
    }

    public override async Task<UpdateUserProfileResponse> UpdateUserProfile(UpdateUserProfileRequest request, ServerCallContext context)
    {
        try
        {
            var updateRequest = new Contracts.Models.UpdateUserProfileRequest
            {
                Name = request.Name,
                Email = request.Email
            };

            var success = await _usersContract.UpdateUserProfileAsync(request.UserId, updateRequest);

            return new UpdateUserProfileResponse
            {
                Success = success,
                Message = success ? "User profile updated successfully" : "Failed to update user profile"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user profile for user {UserId}", request.UserId);
            return new UpdateUserProfileResponse
            {
                Success = false,
                Message = "Error updating user profile"
            };
        }
    }

    public override async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request, ServerCallContext context)
    {
        try
        {
            var success = await _usersContract.DeleteUserAsync(request.UserId);

            return new DeleteUserResponse
            {
                Success = success,
                Message = success ? "User deleted successfully" : "Failed to delete user"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", request.UserId);
            return new DeleteUserResponse
            {
                Success = false,
                Message = "Error deleting user"
            };
        }
    }

    private static UserDto MapToGrpcUserDto(Contracts.Models.UserDto user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            Username = user.UserName,
            Email = user.Email,
            CreatedAt = Timestamp.FromDateTime(user.CreatedAt.ToUniversalTime()),
            UpdatedAt = user.UpdatedAt
        };
    }
}
