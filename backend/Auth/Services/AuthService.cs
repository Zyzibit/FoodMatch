using inzynierka.Auth.Services;
using inzynierka.Auth.Contracts.Models;
using inzynierka.Auth.Model;
using inzynierka.Auth.Repositories;
using inzynierka.EventBus;
using inzynierka.EventBus.Events;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using inzynierka.Auth.EventBus.Events;

namespace inzynierka.Auth.Services;

public class AuthService : IAuthService
{
    private readonly ITokenService _tokenService;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly UserManager<User> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEventBus _eventBus;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        ITokenService tokenService,
        IRefreshTokenRepository refreshTokenRepository,
        UserManager<User> userManager,
        RoleManager<IdentityRole> roleManager,
        SignInManager<User> signInManager,
        IEventBus eventBus,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _tokenService = tokenService;
        _refreshTokenRepository = refreshTokenRepository;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _eventBus = eventBus;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null)
    {
        try
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, false);
            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "Invalid username or password"
                };
            }

            return await GenerateTokensAsync(user, deviceId, userAgent, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user: {Username}", username);
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "Authentication failed"
            };
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password, string? deviceId = null, string? userAgent = null, string? ipAddress = null)
    {
        try
        {
            var existingUser = await _userManager.FindByNameAsync(username);
            if (existingUser != null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "User with this username already exists"
                };
            }

            existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = "User with this email already exists"
                };
            }

            var user = new User
            {
                UserName = username,
                Email = email,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                return new AuthenticationResult
                {
                    Success = false,
                    ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description))
                };
            }

            if (!await _roleManager.RoleExistsAsync(Roles.User))
            {
                _logger.LogWarning("User role does not exist. Creating it now.");
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(Roles.User));
                if (!roleResult.Succeeded)
                {
                    _logger.LogError("Failed to create User role: {Errors}", 
                        string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                }
            }

            var roleAssignResult = await _userManager.AddToRoleAsync(user, Roles.User);
            if (!roleAssignResult.Succeeded)
            {
                _logger.LogWarning("Failed to assign User role to user {UserId}: {Errors}", 
                    user.Id, string.Join(", ", roleAssignResult.Errors.Select(e => e.Description)));
            }


            return await GenerateTokensAsync(user, deviceId, userAgent, ipAddress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Username}", username);
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "Registration failed"
            };
        }
    }

    public async Task<TokenRefreshResult> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (storedRefreshToken == null || 
                storedRefreshToken.RevokedAt != null || 
                storedRefreshToken.ExpiryDate <= DateTime.UtcNow)
            {
                return new TokenRefreshResult
                {
                    Success = false,
                    ErrorMessage = "Invalid refresh token"
                };
            }

            var user = storedRefreshToken.User;
            if (user == null)
            {
                return new TokenRefreshResult
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            storedRefreshToken.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(storedRefreshToken);

            var authResult = await GenerateTokensAsync(user, storedRefreshToken.DeviceId, storedRefreshToken.UserAgent, storedRefreshToken.IpAddress);
            
            if (authResult.Success)
            {
                await _eventBus.PublishAsync(new TokenRefreshedEvent
                {
                    UserId = user.Id,
                    RefreshTime = DateTime.UtcNow
                });

                return new TokenRefreshResult
                {
                    Success = true,
                    AccessToken = authResult.AccessToken,
                    RefreshToken = authResult.RefreshToken,
                    ExpiresAt = authResult.ExpiresAt
                };
            }

            return new TokenRefreshResult
            {
                Success = false,
                ErrorMessage = "Failed to generate new tokens"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return new TokenRefreshResult
            {
                Success = false,
                ErrorMessage = "Token refresh failed"
            };
        }
    }

    public async Task<bool> RevokeTokenAsync(string refreshToken)
    {
        try
        {
            await _refreshTokenRepository.RevokeAsync(refreshToken);
            
            var storedRefreshToken = await _refreshTokenRepository.GetByTokenAsync(refreshToken);
            if (storedRefreshToken != null)
            {
                await _eventBus.PublishAsync(new TokenRevokedEvent
                {
                    UserId = storedRefreshToken.UserId,
                    TokenId = storedRefreshToken.Id.ToString(),
                    RevokeTime = DateTime.UtcNow
                });
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking token");
            return false;
        }
    }

    public async Task<bool> RevokeAllTokensAsync(string userId)
    {
        try
        {
            await _refreshTokenRepository.RevokeAllByUserIdAsync(userId);
            
            await _eventBus.PublishAsync(new UserLoggedOutEvent
            {
                UserId = userId,
                Username = "",
                LogoutTime = DateTime.UtcNow
            });

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error revoking all tokens for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<TokenValidationResult> ValidateTokenAsync(string accessToken)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(accessToken);
            
            var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier)?.Value;
            var username = jsonToken.Claims.FirstOrDefault(x => x.Type == "unique_name")?.Value ?? 
                          jsonToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;
            var roles = jsonToken.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value).ToList();
            var exp = jsonToken.Claims.FirstOrDefault(x => x.Type == "exp")?.Value;

            if (string.IsNullOrEmpty(username))
            {
                return new TokenValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "Invalid token: no username found" 
                };
            }

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return new TokenValidationResult 
                { 
                    IsValid = false, 
                    ErrorMessage = "User not found" 
                };
            }

            var expiresAt = exp != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime : (DateTime?)null;

            await _eventBus.PublishAsync(new TokenValidatedEvent
            {
                UserId = user.Id,
                IsValid = true
            });

            return new TokenValidationResult
            {
                IsValid = true,
                UserId = user.Id,
                Username = username,
                Roles = roles,
                ExpiresAt = expiresAt
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return new TokenValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task<bool> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                // Revoke all refresh tokens for security
                await RevokeAllTokensAsync(userId);
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<List<UserSession>> GetUserSessionsAsync(string userId, string? currentRefreshToken = null)
    {
        try
        {
            var refreshTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);
            
            return refreshTokens.Select(rt => new UserSession
            {
                DeviceId = rt.DeviceId ?? "Unknown",
                UserAgent = rt.UserAgent ?? "Unknown",
                IpAddress = rt.IpAddress ?? "Unknown",
                CreatedAt = rt.CreatedAt,
                ExpiresAt = rt.ExpiryDate,
                IsActive = rt.RevokedAt == null && rt.ExpiryDate > DateTime.UtcNow,
                IsCurrent = !string.IsNullOrEmpty(currentRefreshToken) && rt.Token == currentRefreshToken
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user sessions for userId: {UserId}", userId);
            throw;
        }
    }

    private async Task<AuthenticationResult> GenerateTokensAsync(User user, string? deviceId = null, string? userAgent = null, string? ipAddress = null)
    {
        try
        {
            var roles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName ?? ""),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var accessTokenExpirationMinutes = int.Parse(_configuration["JWT:AccessTokenExpirationMinutes"] ?? "60");
            var refreshTokenExpirationDays = int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7");
            
            var expiresAt = DateTime.UtcNow.AddMinutes(accessTokenExpirationMinutes);

            // Store refresh token
            var refreshTokenEntity = new RefreshToken
            {
                Token = refreshToken,
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(refreshTokenExpirationDays),
                CreatedAt = DateTime.UtcNow,
                DeviceId = deviceId,
                UserAgent = userAgent,
                IpAddress = ipAddress
            };

            if (!string.IsNullOrEmpty(deviceId))
            {
                _logger.LogDebug("Checking for existing token for user {UserId} and device {DeviceId}", user.Id, deviceId);
                var existingToken = await _refreshTokenRepository.GetByUserIdAndDeviceIdAsync(user.Id, deviceId);
                if (existingToken != null)
                {
                    _logger.LogInformation("Revoking existing token {TokenId} for device {DeviceId} of user {UserId}", 
                        existingToken.Id, deviceId, user.Id);
                    await _refreshTokenRepository.RevokeAsync(existingToken.Token);
                }
                else
                {
                    _logger.LogDebug("No existing token found for device {DeviceId} of user {UserId}", deviceId, user.Id);
                }
            }
            else
            {
                _logger.LogWarning("DeviceId is null or empty for user {UserId}", user.Id);
            }

            await _refreshTokenRepository.AddAsync(refreshTokenEntity);
            _logger.LogInformation("Created new refresh token for user {UserId} with device {DeviceId}", user.Id, deviceId);

            await _eventBus.PublishAsync(new UserLoggedInEvent
            {
                UserId = user.Id,
                Username = user.UserName ?? "",
                LoginTime = DateTime.UtcNow
            });

            return new AuthenticationResult
            {
                Success = true,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresAt = expiresAt,
                User = new UserInfo
                {
                    UserId = user.Id,
                    Username = user.UserName ?? "",
                    Email = user.Email ?? "",
                    Roles = roles.ToList()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating tokens for user: {UserId}", user.Id);
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "Failed to generate tokens"
            };
        }
    }
}