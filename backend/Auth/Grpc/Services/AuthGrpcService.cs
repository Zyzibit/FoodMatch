using Grpc.Core;
using inzynierka.Auth.Services;
using inzynierka.Auth.Grpc;
using inzynierka.Auth.Contracts;
using Microsoft.AspNetCore.Identity;
using inzynierka.Auth.Model;
using System.IdentityModel.Tokens.Jwt;

namespace inzynierka.Auth.Grpc.Services;

public class AuthGrpcService : AuthService.AuthServiceBase
{
    private readonly IAuthContract _authModule;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<AuthGrpcService> _logger;

    public AuthGrpcService(
        IAuthContract authModule,
        UserManager<User> userManager,
        ILogger<AuthGrpcService> logger)
    {
        _authModule = authModule;
        _userManager = userManager;
        _logger = logger;
    }

    public override async Task<ValidateTokenResponse> ValidateToken(
        ValidateTokenRequest request,
        ServerCallContext context)
    {
        try
        {
            var result = await _authModule.ValidateTokenAsync(request.Token);

            return new ValidateTokenResponse
            {
                IsValid = result.IsValid,
                UserId = result.UserId ?? "",
                Username = result.Username ?? "",
                Roles = { result.Roles }
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
            var userInfo = await _authModule.GetUserInfoAsync(request.UserId);

            return new GetUserInfoResponse
            {
                UserId = userInfo.UserId,
                Username = userInfo.Username,
                Email = userInfo.Email,
                Roles = { userInfo.Roles }
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
            var result = await _authModule.RefreshTokenAsync(request.RefreshToken);

            return new RefreshTokenResponse
            {
                Success = result.Success,
                AccessToken = result.AccessToken ?? "",
                RefreshToken = result.RefreshToken ?? "",
                ExpiresAt = result.ExpiresAt?.ToFileTimeUtc() ?? 0,
                ErrorMessage = result.ErrorMessage ?? ""
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new RefreshTokenResponse
            {
                Success = false,
                ErrorMessage = "Internal server error"
            };
        }
    }

    public override async Task<RevokeTokenResponse> RevokeToken(
        RevokeTokenRequest request,
        ServerCallContext context)
    {
        try
        {
            var success = await _authModule.RevokeTokenAsync(request.RefreshToken);

            return new RevokeTokenResponse
            {
                Success = success,
                Message = success ? "Token revoked successfully" : "Failed to revoke token"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return new RevokeTokenResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<ChangePasswordResponse> ChangePassword(
        ChangePasswordGrpcRequest request,
        ServerCallContext context)
    {
        try
        {
            var success = await _authModule.ChangePasswordAsync(
                request.UserId,
                request.CurrentPassword,
                request.NewPassword);

            return new ChangePasswordResponse
            {
                Success = success,
                Message = success ? "Password changed successfully" : "Failed to change password"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", request.UserId);
            return new ChangePasswordResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }

    public override async Task<UpdateProfileResponse> UpdateProfile(
        UpdateProfileGrpcRequest request,
        ServerCallContext context)
    {
        try
        {
            var success = await _authModule.UpdateUserProfileAsync(
                request.UserId,
                string.IsNullOrEmpty(request.Email) ? null : request.Email,
                string.IsNullOrEmpty(request.Name) ? null : request.Name);

            return new UpdateProfileResponse
            {
                Success = success,
                Message = success ? "Profile updated successfully" : "Failed to update profile"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user: {UserId}", request.UserId);
            return new UpdateProfileResponse
            {
                Success = false,
                Message = "Internal server error"
            };
        }
    }
}