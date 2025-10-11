/**
 * Client-side utility for managing device identification
 * This should be included in your frontend application
 */

class DeviceManager {
    constructor() {
        this.deviceIdKey = 'device-id';
        this.deviceId = this.getOrCreateDeviceId();
    }

    /**
     * Gets existing device ID from localStorage or creates a new one
     */
    getOrCreateDeviceId() {
        let deviceId = localStorage.getItem(this.deviceIdKey);
        
        if (!deviceId) {
            // Generate a new device ID
            deviceId = this.generateDeviceId();
            localStorage.setItem(this.deviceIdKey, deviceId);
        }
        
        return deviceId;
    }

    /**
     * Generates a unique device ID
     */
    generateDeviceId() {
        return 'dev_' + Math.random().toString(36).substr(2, 9) + Date.now().toString(36);
    }

    /**
     * Gets the current device ID
     */
    getDeviceId() {
        return this.deviceId;
    }

    /**
     * Clears the device ID (useful for logout)
     */
    clearDeviceId() {
        localStorage.removeItem(this.deviceIdKey);
        this.deviceId = this.generateDeviceId();
        localStorage.setItem(this.deviceIdKey, this.deviceId);
    }

    /**
     * Returns headers object with device ID for API requests
     */
    getHeaders() {
        return {
            'X-Device-Id': this.deviceId,
            'Content-Type': 'application/json'
        };
    }
}

// Usage examples:

// Initialize device manager
const deviceManager = new DeviceManager();

// Example: Login request with device ID
async function login(username, password) {
    const response = await fetch('/api/v1/auth/login', {
        method: 'POST',
        headers: deviceManager.getHeaders(),
        body: JSON.stringify({
            username: username,
            password: password
        })
    });
    
    return response.json();
}

// Example: Register request with device ID
async function register(username, email, password) {
    const response = await fetch('/api/v1/auth/register', {
        method: 'POST',
        headers: deviceManager.getHeaders(),
        body: JSON.stringify({
            username: username,
            email: email,
            password: password
        })
    });
    
    return response.json();
}

// Example: Get user sessions
async function getUserSessions() {
    const response = await fetch('/api/v1/auth/sessions', {
        method: 'GET',
        headers: deviceManager.getHeaders(),
        credentials: 'include' // Include cookies
    });
    
    return response.json();
}

// Export for ES6 modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = DeviceManager;
}

// Make available globally
if (typeof window !== 'undefined') {
    window.DeviceManager = DeviceManager;
}