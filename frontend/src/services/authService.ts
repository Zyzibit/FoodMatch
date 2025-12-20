import { API_BASE_URL, API_ENDPOINTS } from "../config";
import type {
  LoginRequest,
  RegisterRequest,
  ChangePasswordRequest,
  AuthenticationResult,
  UserInfo,
  UserSession,
  ApiError,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  ApiMessageResponse,
} from "../types/auth";

class AuthService {
  private baseUrl: string;

  constructor() {
    this.baseUrl = API_BASE_URL;
  }

  private async request<T>(
    endpoint: string,
    options: RequestInit = {}
  ): Promise<T> {
    const url = `${this.baseUrl}${endpoint}`;

    const config: RequestInit = {
      ...options,
      headers: {
        "Content-Type": "application/json",
        ...options.headers,
      },
      credentials: "include", // Important for cookies
    };

    try {
      const response = await fetch(url, config);

      if (!response.ok) {
        const errorData: ApiError = await response.json().catch(() => ({
          message: "An error occurred",
        }));
        throw new Error(
          errorData.message || `HTTP error! status: ${response.status}`
        );
      }

      return await response.json();
    } catch (error) {
      if (error instanceof Error) {
        throw error;
      }
      throw new Error("An unexpected error occurred");
    }
  }

  async login(
    username: string,
    password: string
  ): Promise<AuthenticationResult> {
    const payload: LoginRequest = { username, password };

    return this.request<AuthenticationResult>(API_ENDPOINTS.AUTH.LOGIN, {
      method: "POST",
      body: JSON.stringify(payload),
    });
  }

  async register(
    username: string,
    email: string,
    password: string
  ): Promise<AuthenticationResult> {
    const payload: RegisterRequest = { username, email, password };

    return this.request<AuthenticationResult>(API_ENDPOINTS.AUTH.REGISTER, {
      method: "POST",
      body: JSON.stringify(payload),
    });
  }

  async refreshToken(): Promise<void> {
    // Backend reads refresh token from httpOnly cookie
    return this.request<void>(API_ENDPOINTS.AUTH.REFRESH_TOKEN, {
      method: "POST",
      body: JSON.stringify({}),
    });
  }

  async logout(): Promise<void> {
    // Backend reads refresh token from httpOnly cookie and invalidates it
    return this.request<void>(API_ENDPOINTS.AUTH.LOGOUT, {
      method: "POST",
      body: JSON.stringify({}),
    });
  }

  async validateToken(): Promise<{ isValid: boolean }> {
    return this.request<{ isValid: boolean }>(
      API_ENDPOINTS.AUTH.VALIDATE_TOKEN,
      {
        method: "POST",
      }
    );
  }

  async getCurrentUser(): Promise<UserInfo> {
    return this.request<UserInfo>(API_ENDPOINTS.AUTH.ME, {
      method: "GET",
    });
  }

  async getUserInfo(userId: string): Promise<UserInfo> {
    return this.request<UserInfo>(`${API_ENDPOINTS.AUTH.USER}/${userId}`, {
      method: "GET",
    });
  }

  async changePassword(
    currentPassword: string,
    newPassword: string
  ): Promise<{ message: string }> {
    const payload: ChangePasswordRequest = { currentPassword, newPassword };

    return this.request<{ message: string }>(
      API_ENDPOINTS.AUTH.CHANGE_PASSWORD,
      {
        method: "POST",
        body: JSON.stringify(payload),
      }
    );
  }

  async forgotPassword(email: string): Promise<ApiMessageResponse> {
    const payload: ForgotPasswordRequest = { email };

    return this.request<ApiMessageResponse>(API_ENDPOINTS.AUTH.FORGOT_PASSWORD, {
      method: "POST",
      body: JSON.stringify(payload),
    });
  }

  async resetPassword(
    email: string,
    token: string,
    newPassword: string
  ): Promise<ApiMessageResponse> {
    const payload: ResetPasswordRequest = { email, token, newPassword };

    return this.request<ApiMessageResponse>(API_ENDPOINTS.AUTH.RESET_PASSWORD, {
      method: "POST",
      body: JSON.stringify(payload),
    });
  }

  async getSessions(): Promise<UserSession[]> {
    return this.request<UserSession[]>(API_ENDPOINTS.AUTH.SESSIONS, {
      method: "GET",
    });
  }

  async revokeAllTokens(): Promise<{ message: string }> {
    return this.request<{ message: string }>(
      API_ENDPOINTS.AUTH.REVOKE_ALL_TOKENS,
      {
        method: "POST",
      }
    );
  }
}

export const authService = new AuthService();
export default authService;
