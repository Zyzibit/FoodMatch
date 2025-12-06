// Aspire ustawia VITE_API_BASE_URL na pełny URL backendu (np. https://localhost:7257)
const backendUrl = import.meta.env.VITE_API_BASE_URL || "https://localhost:7257";
export const API_BASE_URL = `${backendUrl}/api/v1`;

export const API_ENDPOINTS = {
  AUTH: {
    LOGIN: "/auth/login",
    REGISTER: "/auth/register",
    REFRESH_TOKEN: "/auth/refresh-token",
    LOGOUT: "/auth/logout",
    ME: "/auth/me",
    VALIDATE_TOKEN: "/auth/validate-token",
    USER: "/auth/user",
    CHANGE_PASSWORD: "/auth/change-password",
    SESSIONS: "/auth/sessions",
    REVOKE_ALL_TOKENS: "/auth/revoke-all-tokens",
  },
  PRODUCTS: {
    BASE: "/products",
    SEARCH: "/products/search",
  },
  USERS: {
    ME: "/users/me",
  },
} as const;
