using inzynierka.Users.Contracts;
using inzynierka.Users.Contracts.Models;
using inzynierka.Users.Services;
using inzynierka.Users.Model;

namespace inzynierka.Users.Modules;

public class UsersModule : IUsersContract
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersModule> _logger;

    public UsersModule(IUserService userService, ILogger<UsersModule> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(string username)
    {
        var user = await _userService.GetUserByUsernameAsync(username);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<UserDto?> GetUserByEmailAsync(string email)
    {
        var user = await _userService.GetUserByEmailAsync(email);
        return user != null ? MapToUserDto(user) : null;
    }

    public async Task<List<UserDto>> GetUsersAsync(int pageNumber = 1, int pageSize = 10)
    {
        var users = await _userService.GetUsersAsync(pageNumber, pageSize);
        return users.Select(MapToUserDto).ToList();
    }

    public async Task<bool> UpdateUserProfileAsync(string userId, UpdateUserProfileRequest request)
    {
        return await _userService.UpdateUserProfileAsync(userId, request.Name, request.Email);
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        return await _userService.DeleteUserAsync(userId);
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _userService.GetTotalUsersCountAsync();
    }

    private static UserDto MapToUserDto(UserProfile user)
    {
        return new UserDto
        {
            Id = user.Id,
            Name = user.Name,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}
