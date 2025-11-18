// Request types
export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
}

export interface RefreshTokenRequest {
  refreshToken?: string;
}

export interface LogoutRequest {
  refreshToken?: string;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
}

// Response types
export interface UserInfo {
  userId: string;
  username: string;
  email: string;
  roles?: string[];
  createdAt?: string;
}

export interface AuthenticationResult {
  success: boolean;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserInfo;
}

export interface TokenRefreshResult {
  success: boolean;
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface UserSession {
  deviceId: string;
  userAgent: string;
  ipAddress: string;
  lastUsed: string;
  isCurrentDevice: boolean;
}

export interface ApiError {
  message: string;
}
