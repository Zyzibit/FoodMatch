import { API_BASE_URL, API_ENDPOINTS } from "../config";
import type { UserInfo } from "../types/auth";

export interface UpdateUserProfileRequest {
  name?: string;
  email?: string;
}

class UserService {
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
      credentials: "include",
    };

    try {
      const response = await fetch(url, config);

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({
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

  async getCurrentUserProfile(): Promise<UserInfo> {
    return this.request<UserInfo>(API_ENDPOINTS.USERS.ME, {
      method: "GET",
    });
  }

  async updateCurrentUserProfile(
    data: UpdateUserProfileRequest
  ): Promise<{ message: string }> {
    return this.request<{ message: string }>(API_ENDPOINTS.USERS.ME, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  }

  async uploadProfilePicture(
    file: File
  ): Promise<{ message: string; profilePictureUrl: string }> {
    const formData = new FormData();
    formData.append("file", file);

    const url = `${this.baseUrl}/users/profile-picture`;
    const response = await fetch(url, {
      method: "POST",
      body: formData,
      credentials: "include",
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({
        message: "Failed to upload profile picture",
      }));
      throw new Error(errorData.message || "Failed to upload profile picture");
    }

    return await response.json();
  }

  async deleteProfilePicture(): Promise<{ message: string }> {
    const url = `${this.baseUrl}/users/profile-picture`;
    const response = await fetch(url, {
      method: "DELETE",
      credentials: "include",
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({
        message: "Failed to delete profile picture",
      }));
      throw new Error(errorData.message || "Failed to delete profile picture");
    }

    return await response.json();
  }
}

export const userService = new UserService();
export default userService;
