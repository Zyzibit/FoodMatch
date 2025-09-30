using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using inzynierka.Auth.Model;
using inzynierka.Auth.Model.DTO;
using inzynierka.Auth.Services;
using inzynierka.Data;
using inzynierka.Products.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace inzynierka.Auth.Controller;

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogger<AuthController> _logger;
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager,
            ILogger<AuthController> logger,
            ITokenService tokenService, AppDbContext context, IConfiguration configuration) {
            _userManager = userManager;
            _roleManager = roleManager;
            _logger = logger;
            _tokenService = tokenService;
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup(SignupModel model) {
            try {
                var existingUser = await _userManager.FindByNameAsync(model.Email);
                if (existingUser != null) return BadRequest("User already exists");

                // Create Products role if it doesn't exist
                if (await _roleManager.RoleExistsAsync(Roles.User) == false) {
                    var roleResult = await _roleManager
                        .CreateAsync(new IdentityRole(Roles.User));

                    if (roleResult.Succeeded == false) {
                        var roleErros = roleResult.Errors.Select(e => e.Description);
                        _logger.LogError($"Failed to create user role. Errors : {string.Join(",", roleErros)}");
                        return BadRequest($"Failed to create user role. Errors : {string.Join(",", roleErros)}");
                    }
                }

                User user = new() {
                    Email = model.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = model.Email,
                    Name = model.Name,
                    EmailConfirmed = true
                };

                // Attempt to create a user
                var createUserResult = await _userManager.CreateAsync(user, model.Password);

                // Validate user creation. If user is not created, log the error and
                // return the BadRequest along with the errors
                if (createUserResult.Succeeded == false) {
                    var errors = createUserResult.Errors.Select(e => e.Description);
                    _logger.LogError(
                        $"Failed to create user. Errors: {string.Join(", ", errors)}"
                    );
                    return BadRequest($"Failed to create user. Errors: {string.Join(", ", errors)}");
                }

                // adding role to user
                var addUserToRoleResult = await _userManager.AddToRoleAsync(user, Roles.User);

                if (addUserToRoleResult.Succeeded == false) {
                    var errors = addUserToRoleResult.Errors.Select(e => e.Description);
                    _logger.LogError($"Failed to add role to the user. Errors : {string.Join(",", errors)}");
                }

                return CreatedAtAction(nameof(Signup), null);
            }
            catch (Exception ex) {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginModel model) {
            try {
                var user = await _userManager.FindByNameAsync(model.Username);
                if (user == null) return BadRequest("User with this username is not registered with us.");
                var isValidPassword = await _userManager.CheckPasswordAsync(user, model.Password);
                if (isValidPassword == false) return Unauthorized();

                List<Claim> authClaims = [
                    new(ClaimTypes.Name, user.UserName),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                ];

                var userRoles = await _userManager.GetRolesAsync(user);

                foreach (var userRole in userRoles) authClaims.Add(new Claim(ClaimTypes.Role, userRole));

                var accessToken = _tokenService.GenerateAccessToken(authClaims);

                var refreshToken = _tokenService.GenerateRefreshToken();

                var tokenInfo = _context.TokenInfos.FirstOrDefault(a => a.Username == user.UserName);

                if (tokenInfo == null) {
                    var ti = new TokenInfo {
                        Username = user.UserName,
                        RefreshToken = refreshToken,
                        ExpiredAt = DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"])),
                        Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString(),
                    };
                    _context.TokenInfos.Add(ti);
                }
                else {
                    tokenInfo.RefreshToken = refreshToken;
                    tokenInfo.ExpiredAt =
                        DateTime.UtcNow.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]));
                    tokenInfo.Ip = Request.HttpContext.Connection.RemoteIpAddress?.ToString();
                }

                await _context.SaveChangesAsync();
                _tokenService.SetRefreshTokenCookie(Response, refreshToken,
                    int.Parse(_configuration["Cookie:ExpirationDays"]));
                _tokenService.SetAccessTokenCookie(Response, accessToken, int.Parse(_configuration["Cookie:ExpirationMinutes"]));
                return Ok(new TokenModel {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken
                });
            }
            catch (Exception ex) {
                _logger.LogError(ex.Message);
                return Unauthorized();
            }
        }
        
        
        /// <summary>
        /// Refreshes the access token using the refresh token stored in cookies.
        /// </summary>
        /// <returns>
        /// An IActionResult containing the new access token if the refresh is successful,
        /// or an error message if the refresh token is invalid or expired.
        /// </returns>
        /// <remarks>
        /// This method checks for the presence of a refresh token in the cookies and an access token
        /// in the Authorization header. It validates the access token and refresh token, and if valid,
        /// generates new tokens and updates the database and cookies accordingly.
        /// </remarks>
    [HttpPost("token/refresh")]
    public async Task<IActionResult> Refresh()
    {
        try
        {
            if (!Request.Cookies.TryGetValue("RefreshToken", out var oldRefreshToken))
            {
                return Unauthorized("Refresh token not found in cookies.");
            }

            var tokenInfo = await _context.TokenInfos
                .SingleOrDefaultAsync(t => t.RefreshToken == oldRefreshToken);

            if (tokenInfo == null || tokenInfo.ExpiredAt <= DateTime.UtcNow)
            {
                Response.Cookies.Delete("RefreshToken");
                Response.Cookies.Delete("AccessToken");
                return Unauthorized("Invalid or expired refresh token. Please login again.");
            }

            var username = tokenInfo.Username;
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                return Unauthorized("User not found.");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, user.UserName),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var newAccessToken = _tokenService.GenerateAccessToken(claims);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            tokenInfo.RefreshToken = newRefreshToken;
            tokenInfo.ExpiredAt = DateTime.UtcNow
                .AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpirationDays"]));

            await _context.SaveChangesAsync();

            _tokenService.SetAccessTokenCookie(
                Response,
                newAccessToken,
                int.Parse(_configuration["Cookie:ExpirationMinutes"]));
            _tokenService.SetRefreshTokenCookie(
                Response,
                newRefreshToken,
                int.Parse(_configuration["Cookie:ExpirationDays"]));

            return Ok(new TokenModel
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Unexpected error in token refresh: {ex.Message}");
            return StatusCode(StatusCodes.Status500InternalServerError);
        }
    }


        [HttpPost("token/revoke")]
        public async Task<IActionResult> Revoke() 
        {
            try 
            {
                if (!Request.Cookies.TryGetValue("RefreshToken", out var refreshTokenFromCookie))
                    return BadRequest("Refresh token not found in cookies.");

                if (!Request.Cookies.TryGetValue("AccessToken", out var accessToken))
                    return BadRequest("Access token not found in cookies.");

                var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);

                if (principal == null)
                    return Unauthorized("Invalid access token.");

                var username = principal.Identity?.Name;

                if (string.IsNullOrEmpty(username))
                    return BadRequest("Invalid access token. Username not found.");

                var userTokenInfo = await _context.TokenInfos.SingleOrDefaultAsync(u => u.Username == username);

                if (userTokenInfo == null)
                    return BadRequest("User not found in the database.");

                if (userTokenInfo.RefreshToken != refreshTokenFromCookie)
                    return BadRequest("Refresh token mismatch between cookie and database.");

                userTokenInfo.RefreshToken = string.Empty;
                await _context.SaveChangesAsync();
                _tokenService.RemoveRefreshTokenCookie(Response);
                _tokenService.RemoveAccessTokenCookie(Response);

                return Ok(true);
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }