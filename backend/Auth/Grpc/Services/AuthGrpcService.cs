using Grpc.Core;
using inzynierka.Auth.Services;
using inzynierka.Auth.Grpc;
using Microsoft.AspNetCore.Identity;
using inzynierka.Auth.Model;
using System.IdentityModel.Tokens.Jwt;

namespace inzynierka.Auth.Grpc.Services;

public class AuthGrpcService : AuthService.AuthServiceBase
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthGrpcService> _logger;

    public AuthGrpcService(
        ITokenService tokenService,
        UserManager<User> userManager,
        ILogger<AuthGrpcService> logger)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task<ValidateTokenResponse> ValidateToken(
        ValidateTokenRequest request, 
        ServerCallContext context)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(request.Token);
            
            var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
            var username = jsonToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value;
            var roles = jsonToken.Claims.Where(x => x.Type == "role").Select(x => x.Value).ToList();

            if (userId == null)
            {
                return new ValidateTokenResponse { IsValid = false };
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return new ValidateTokenResponse { IsValid = false };
            }

            return new ValidateTokenResponse
            {
                IsValid = true,
                UserId = userId,
                Username = username ?? user.UserName ?? "",
                Roles = { roles }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return new ValidateTokenResponse { IsValid = false };
        }
    }

    public override async Task<GetUserInfoResponse> GetUserInfo(
        GetUserInfoRequest request, 
        ServerCallContext context)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "User not found"));
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new GetUserInfoResponse
            {
                UserId = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                Roles = { roles }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for userId: {UserId}", request.UserId);
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }

    public override async Task<RefreshTokenResponse> RefreshToken(
        RefreshTokenRequest request, 
        ServerCallContext context)
    {
        try
        {
            // Note: This is a simplified implementation
            // You might want to implement proper refresh token logic
            throw new RpcException(new Status(StatusCode.Unimplemented, "Refresh token not implemented yet"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            throw new RpcException(new Status(StatusCode.Internal, "Internal server error"));
        }
    }
}