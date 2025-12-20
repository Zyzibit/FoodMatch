namespace inzynierka.Auth.Utilities;

/// <summary>
/// Helper class for extracting device information from HTTP requests
/// </summary>
public static class DeviceInfoHelper
{
    public static (string deviceId, string? userAgent, string? ipAddress) ExtractDeviceInfo(HttpRequest request)
    {
        var deviceId = request.Headers["X-Device-Id"].FirstOrDefault() ?? 
                      request.Headers["Device-Id"].FirstOrDefault() ??
                      request.Headers["X-Client-Id"].FirstOrDefault();

        var userAgent = request.Headers["User-Agent"].FirstOrDefault();
        var ipAddress = GetRealIpAddress(request);

        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = CreateDeviceFingerprint(request);
        }
        return (deviceId, userAgent, ipAddress);
    }

    
    public static string CreateDeviceFingerprint(HttpRequest request)
    {
        var userAgent = request.Headers["User-Agent"].FirstOrDefault() ?? "";
        var ipAddress = GetRealIpAddress(request) ?? "";

        var fingerprint = $"{userAgent}|{ipAddress}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();

        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(fingerprint));
        return Convert.ToHexString(hashBytes)[..16]; 
    }


    public static string? GetRealIpAddress(HttpRequest request)
    {
        var forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }
        var realIp = request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        var cfIp = request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfIp))
        {
            return cfIp;
        }

        return request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    public static string CreateDetailedFingerprint(HttpRequest request)
    {
        var userAgent = request.Headers["User-Agent"].FirstOrDefault() ?? "";
        var acceptLanguage = request.Headers["Accept-Language"].FirstOrDefault() ?? "";
        var acceptEncoding = request.Headers["Accept-Encoding"].FirstOrDefault() ?? "";
        var ipAddress = GetRealIpAddress(request) ?? "";
        
        return $"UA:{userAgent}|IP:{ipAddress}|AL:{acceptLanguage}|AE:{acceptEncoding}";
    }

    public static string GenerateRandomDeviceId()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }
}