# Device Information Tracking Fix

This fix addresses the issue where user sessions were showing "Unknown" values for `deviceId`, `userAgent`, and `ipAddress`, and prevents multiple sessions from being created for the same device.

## What Was Fixed

1. **AuthController**: Updated to extract device information from HTTP request headers with improved logging
2. **AuthContract & AuthModule**: Updated to accept and pass device information parameters
3. **DeviceInfoHelper**: Enhanced utility class with consistent device fingerprinting
4. **Device Token Management**: Improved logic to replace existing tokens for the same device
5. **Client-side integration**: JavaScript utility for managing device IDs on the frontend
6. **Debug endpoint**: Added endpoint for troubleshooting device identification

## How It Works

### Backend Changes

The authentication flow now captures:
- **Device ID**: From headers or generates consistent fingerprint based on User-Agent + IP address
- **User Agent**: From the `User-Agent` header
- **IP Address**: From proxy headers or connection IP
- **Token Replacement**: Automatically revokes old tokens when same device logs in again

### Device Information Priority

1. `X-Device-Id` header (recommended - explicit device ID)
2. `Device-Id` header  
3. `X-Client-Id` header
4. **Device Fingerprint** (User-Agent + IP Address hash) - prevents duplicate sessions

### IP Address Priority

1. `X-Forwarded-For` header (first IP in comma-separated list)
2. `X-Real-IP` header (nginx)
3. `CF-Connecting-IP` header (Cloudflare)
4. Connection remote IP address

## Device Fingerprinting

When no explicit device ID is provided, the system creates a consistent fingerprint using:
- **User-Agent**: Browser/client identification
- **IP Address**: Network location
- **SHA256 Hash**: Creates 16-character consistent identifier

This ensures the same device gets the same ID across requests, preventing multiple sessions.

## Frontend Integration

Include the `device-manager.js` file in your frontend application:

```javascript
// Initialize device manager
const deviceManager = new DeviceManager();

// Use in API calls
const response = await fetch('/api/v1/auth/login', {
    method: 'POST',
    headers: deviceManager.getHeaders(),
    body: JSON.stringify({ username, password })
});
```

## Debugging

### Debug Endpoint

Use the debug endpoint to check device identification:

```
GET /api/v1/auth/debug/device-info
Authorization: Bearer <your-token>
```

Response:
```json
{
    "deviceId": "A1B2C3D4E5F67890",
    "userAgent": "PostmanRuntime/7.48.0",
    "ipAddress": "::1",
    "detailedFingerprint": "UA:PostmanRuntime/7.48.0|IP:::1|AL:|AE:",
    "headers": {
        "User-Agent": "PostmanRuntime/7.48.0",
        "X-Device-Id": "custom-device-id"
    }
}
```

### Logging

The system now logs device information during authentication:

```
[INFO] Login attempt for user testuser from device A1B2C3D4E5F67890 (IP: ::1)
[INFO] Revoking existing token 123 for device A1B2C3D4E5F67890 of user user-id-123
[INFO] Created new refresh token for user user-id-123 with device A1B2C3D4E5F67890
```

## Headers Expected by Backend

```javascript
{
    'X-Device-Id': 'unique-device-identifier', // Optional - if not provided, fingerprint is used
    'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64)...',
    // IP is automatically extracted from connection/proxy headers
}
```

## Example User Session Response

After the fix, user sessions will show actual device information and properly manage device-based sessions:

```json
[
    {
        "deviceId": "A1B2C3D4E5F67890",
        "userAgent": "PostmanRuntime/7.48.0",
        "ipAddress": "::1",
        "createdAt": "2025-01-07T20:25:23.356825Z",
        "expiresAt": "2025-01-14T20:25:23.356825Z",
        "isActive": true,
        "isCurrent": true
    }
]
```

## Testing Multiple Logins from Same Device

1. **Without X-Device-Id header**: System will generate same fingerprint for same User-Agent + IP
2. **With X-Device-Id header**: System will use explicit device ID
3. **Same device, multiple logins**: Only one active session per device (old ones are revoked)

## Troubleshooting

### Multiple Sessions Still Created?

1. Check if User-Agent changes between requests
2. Verify IP address consistency (especially with proxies)
3. Use explicit `X-Device-Id` header for guaranteed consistency
4. Check debug endpoint to see what device ID is being generated

### "Unknown" Values Still Appearing?

1. Ensure clients send User-Agent header
2. Check proxy configuration for IP forwarding
3. Verify new login is being performed (old sessions may still show "Unknown")

## Security Benefits

- **Better session management**: One session per device prevents session proliferation
- **Enhanced audit trail**: Track user activities by device
