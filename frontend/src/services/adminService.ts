import { API_BASE_URL } from "../config";

const getAuthHeaders = () => ({
  "Content-Type": "application/json",
});

export interface AdminUserInfo {
  id: string;
  username: string;
  email: string;
  roles?: string[];
  createdAt: string;
  isActive?: boolean;
}

export interface UserListResult {
  users: AdminUserInfo[];
  totalCount: number;
  limit: number;
  offset: number;
  hasMore: boolean;
}

export const getAllUsers = async (
  limit: number = 50,
  offset: number = 0
): Promise<UserListResult> => {
  const pageNumber = Math.floor(offset / limit) + 1;
  const response = await fetch(
    `${API_BASE_URL}/users?pageNumber=${pageNumber}&pageSize=${limit}`,
    {
      method: "GET",
      headers: getAuthHeaders(),
      credentials: "include",
    }
  );

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się pobrać użytkowników" }));
    throw new Error(error.message || "Nie udało się pobrać użytkowników");
  }

  const data = await response.json();
  return {
    users: Array.isArray(data.users) 
      ? data.users.map((u: any) => ({
          id: u.id,
          username: u.userName || u.username,
          email: u.email,
          roles: u.roles || [],
          createdAt: u.createdAt,
          isActive: true,
        }))
      : [],
    totalCount: data.totalCount || 0,
    limit: data.pageSize || limit,
    offset: (data.pageNumber - 1) * data.pageSize || offset,
    hasMore: data.pageNumber < data.totalPages,
  };
};

export const getUserById = async (userId: string): Promise<AdminUserInfo> => {
  const response = await fetch(`${API_BASE_URL}/users/${userId}`, {
    method: "GET",
    headers: getAuthHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się pobrać użytkownika" }));
    throw new Error(error.message || "Nie udało się pobrać użytkownika");
  }

  return await response.json();
};

export const deleteUser = async (userId: string): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/users/${userId}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się usunąć użytkownika" }));
    throw new Error(error.message || "Nie udało się usunąć użytkownika");
  }
};

export interface UpdateUserRequest {
  name?: string;
  email?: string;
}

export const updateUser = async (
  userId: string,
  data: UpdateUserRequest
): Promise<void> => {
  const response = await fetch(`${API_BASE_URL}/users/${userId}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    credentials: "include",
    body: JSON.stringify(data),
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się zaktualizować użytkownika" }));
    throw new Error(error.message || "Nie udało się zaktualizować użytkownika");
  }
};
