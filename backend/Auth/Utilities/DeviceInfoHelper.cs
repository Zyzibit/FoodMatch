namespace inzynierka.Auth.Utilities;

/// <summary>
/// Helper class for extracting device information from HTTP requests
/// </summary>
public static class DeviceInfoHelper
{
    /// <summary>
    /// Extracts device information from HTTP request headers
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <param name="logger">Optional logger for debugging</param>
    /// <returns>A tuple containing deviceId, userAgent, and ipAddress</returns>
    public static (string deviceId, string? userAgent, string? ipAddress) ExtractDeviceInfo(HttpRequest request, ILogger? logger = null)
    {
        // Try to get device ID from various headers
        var deviceId = request.Headers["X-Device-Id"].FirstOrDefault() ?? 
                      request.Headers["Device-Id"].FirstOrDefault() ??
                      request.Headers["X-Client-Id"].FirstOrDefault();

        var userAgent = request.Headers["User-Agent"].FirstOrDefault();
        var ipAddress = GetRealIpAddress(request);

        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = CreateDeviceFingerprint(request);
            logger?.LogDebug("Generated device fingerprint: {DeviceId} for UserAgent: {UserAgent}, IP: {IpAddress}", 
                deviceId, userAgent, ipAddress);
        }
        else
        {
            logger?.LogDebug("Using explicit device ID: {DeviceId}", deviceId);
        }

        return (deviceId, userAgent, ipAddress);
    }

    /// <summary>
    /// Gets the real IP address from the request, considering proxy headers
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <returns>The real IP address or null if not found</returns>
    public static string? GetRealIpAddress(HttpRequest request)
    {
        // Check X-Forwarded-For header (common for load balancers/proxies)
        var forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            // Take the first IP from the comma-separated list
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        // Check X-Real-IP header (nginx)
        var realIp = request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // Check CF-Connecting-IP header (Cloudflare)
        var cfIp = request.Headers["CF-Connecting-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(cfIp))
        {
            return cfIp;
        }

        // Fall back to connection remote IP
        return request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }

    /// <summary>
    /// Creates a device fingerprint based on available request information
    /// This ensures the same device gets the same ID consistently
    /// Uses only the most stable headers for consistency
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <returns>A consistent device fingerprint string</returns>
    public static string CreateDeviceFingerprint(HttpRequest request)
    {
        var userAgent = request.Headers["User-Agent"].FirstOrDefault() ?? "";
        var ipAddress = GetRealIpAddress(request) ?? "";
        
        // Only use the most stable headers - avoid Accept-Language and Accept-Encoding 
        // as they can vary between requests from the same client
        var fingerprint = $"{userAgent}|{ipAddress}";
        
        // Create a hash of the fingerprint for consistency
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hashBytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(fingerprint));
        return Convert.ToHexString(hashBytes)[..16]; // Take first 16 characters
    }

    /// <summary>
    /// Creates a more detailed device fingerprint for debugging purposes
    /// </summary>
    /// <param name="request">The HTTP request</param>
    /// <returns>A detailed fingerprint with all headers</returns>
    public static string CreateDetailedFingerprint(HttpRequest request)
    {
        var userAgent = request.Headers["User-Agent"].FirstOrDefault() ?? "";
        var acceptLanguage = request.Headers["Accept-Language"].FirstOrDefault() ?? "";
        var acceptEncoding = request.Headers["Accept-Encoding"].FirstOrDefault() ?? "";
        var ipAddress = GetRealIpAddress(request) ?? "";
        
        return $"UA:{userAgent}|IP:{ipAddress}|AL:{acceptLanguage}|AE:{acceptEncoding}";
    }

    /// <summary>
    /// Generates a truly random device ID (used only when needed)
    /// </summary>
    /// <returns>A 16-character random device ID</returns>
    public static string GenerateRandomDeviceId()
    {
        return Guid.NewGuid().ToString("N")[..16];
    }
}