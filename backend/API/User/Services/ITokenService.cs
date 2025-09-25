using System.Security.Claims;

namespace inzynierka.API.User.Services;

public interface ITokenService
{
    string GenerateAccessToken(IEnumerable<Claim> claims);
    string GenerateRefreshToken();
    ClaimsPrincipal GetPrincipalFromExpiredToken(string accessToken);
    void RemoveRefreshTokenCookie(HttpResponse response);
    void SetRefreshTokenCookie(HttpResponse response, string refreshToken, int days);
    void RemoveAccessTokenCookie(HttpResponse response);
    void SetAccessTokenCookie(HttpResponse response, string accessToken, int minutes);
}