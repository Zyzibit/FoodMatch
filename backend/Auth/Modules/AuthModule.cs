using inzynierka.Auth.Contracts;
using inzynierka.Auth.Contracts.Models;
using inzynierka.Auth.Services;
using inzynierka.Auth.Model;
using inzynierka.Auth.EventBus;
using inzynierka.EventBus;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using inzynierka.Auth.EventBus.Events;

namespace inzynierka.Auth.Modules;

/// <summary>
/// Implementacja kontraktu autoryzacji - modu³ Auth
/// </summary>
public class AuthModule : IAuthContract
{
    private readonly ITokenService _tokenService;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly IEventBus _eventBus;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthModule> _logger;

    public AuthModule(
        ITokenService tokenService,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IEventBus eventBus,
        IConfiguration configuration,
        ILogger<AuthModule> logger)
    {
        _tokenService = tokenService;
        _userManager = userManager;
        _signInManager = signInManager;
        _eventBus = eventBus;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenValidationResult> ValidateTokenAsync(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jsonToken = tokenHandler.ReadJwtToken(token);
            
            var userId = jsonToken.Claims.FirstOrDefault(x => x.Type == "nameid")?.Value;
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

            // Publikacja zdarzenia walidacji tokenu
            await _eventBus.PublishAsync(new TokenValidatedEvent
            {
                UserId = user.Id,
                IsValid = true
            });

            var expiresAt = exp != null ? DateTimeOffset.FromUnixTimeSeconds(long.Parse(exp)).DateTime : (DateTime?)null;

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
            
            await _eventBus.PublishAsync(new TokenValidatedEvent
            {
                UserId = "",
                IsValid = false
            });

            return new TokenValidationResult 
            { 
                IsValid = false, 
                ErrorMessage = ex.Message 
            };
        }
    }

    public async Task<UserInfo> GetUserInfoAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                throw new InvalidOperationException("User not found");
            }

            var roles = await _userManager.GetRolesAsync(user);

            return new UserInfo
            {
                UserId = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                Roles = roles.ToList(),
                CreatedAt = DateTime.UtcNow,
                LastLoginAt = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user info for userId: {UserId}", userId);
            throw;
        }
    }

    public async Task<AuthenticationResult> AuthenticateAsync(string username, string password)
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

            var expirationMinutes = double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            // Publikacja zdarzenia logowania
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
            _logger.LogError(ex, "Error during authentication for user: {Username}", username);
            return new AuthenticationResult
            {
                Success = false,
                ErrorMessage = "Authentication failed"
            };
        }
    }

    public async Task<AuthenticationResult> RegisterAsync(string username, string email, string password)
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

            await _userManager.AddToRoleAsync(user, Roles.User);

            var roles = await _userManager.GetRolesAsync(user);
            
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var accessToken = _tokenService.GenerateAccessToken(claims);
            var refreshToken = _tokenService.GenerateRefreshToken();

            var expirationMinutes = double.Parse(_configuration["Jwt:AccessTokenExpirationMinutes"] ?? "60");
            var expiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes);

            // Publikacja zdarzenia rejestracji
            await _eventBus.PublishAsync(new UserRegisteredEvent
            {
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email
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
                    Username = user.UserName,
                    Email = user.Email,
                    Roles = roles.ToList()
                }
            };
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
        await Task.CompletedTask;
        return new TokenRefreshResult
        {
            Success = false,
            ErrorMessage = "Refresh token functionality not implemented yet"
        };
    }

    public async Task<bool> RevokeTokenAsync(string token)
    {
        await Task.CompletedTask;
        return false;
    }
}