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
  user: UserInfo;
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
