import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from "react";
import authService from "../services/authService";
import type { UserInfo, UserSession } from "../types/auth";

interface AuthContextType {
  user: UserInfo | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  login: (username: string, password: string) => Promise<void>;
  register: (
    username: string,
    email: string,
    password: string
  ) => Promise<void>;
  logout: () => Promise<void>;
  refreshToken: () => Promise<void>;
  clearError: () => void;
  getCurrentUser: () => Promise<UserInfo>;
  changePassword: (
    currentPassword: string,
    newPassword: string
  ) => Promise<void>;
  getSessions: () => Promise<UserSession[]>;
  revokeAllTokens: () => Promise<void>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};

interface AuthProviderProps {
  children: ReactNode;
}

export const AuthProvider: React.FC<AuthProviderProps> = ({ children }) => {
  const [user, setUser] = useState<UserInfo | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Store tokens in state (they're also in httpOnly cookies)
  const [accessToken, setAccessToken] = useState<string | null>(null);
  const [refreshTokenValue, setRefreshTokenValue] = useState<string | null>(
    null
  );
  const [tokenExpiresAt, setTokenExpiresAt] = useState<string | null>(null);

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    try {
      setIsLoading(true);
      setError(null);

      const result = await authService.login(username, password);

      setUser(result.user);
      setAccessToken(result.accessToken);
      setRefreshTokenValue(result.refreshToken);
      setTokenExpiresAt(result.expiresAt);

      // Store in localStorage for persistence
      localStorage.setItem("accessToken", result.accessToken);
      localStorage.setItem("refreshToken", result.refreshToken);
      localStorage.setItem("tokenExpiresAt", result.expiresAt);
      localStorage.setItem("user", JSON.stringify(result.user));
    } catch (err) {
      const errorMessage = err instanceof Error ? err.message : "Login failed";
      setError(errorMessage);
      throw err;
    } finally {
      setIsLoading(false);
    }
  }, []);

  const register = useCallback(
    async (username: string, email: string, password: string) => {
      try {
        setIsLoading(true);
        setError(null);

        const result = await authService.register(username, email, password);

        setUser(result.user);
        setAccessToken(result.accessToken);
        setRefreshTokenValue(result.refreshToken);
        setTokenExpiresAt(result.expiresAt);

        // Store in localStorage for persistence
        localStorage.setItem("accessToken", result.accessToken);
        localStorage.setItem("refreshToken", result.refreshToken);
        localStorage.setItem("tokenExpiresAt", result.expiresAt);
        localStorage.setItem("user", JSON.stringify(result.user));
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Registration failed";
        setError(errorMessage);
        throw err;
      } finally {
        setIsLoading(false);
      }
    },
    []
  );

  const logout = useCallback(async () => {
    try {
      // Clear state first to prevent any race conditions
      setUser(null);
      setAccessToken(null);
      setRefreshTokenValue(null);
      setTokenExpiresAt(null);
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("tokenExpiresAt");
      localStorage.removeItem("user");

      // Then call backend to invalidate tokens
      await authService.logout();
    } catch (err) {
      console.error("Logout error:", err);
      // Even if backend call fails, ensure local state is cleared
      setUser(null);
      setAccessToken(null);
      setRefreshTokenValue(null);
      setTokenExpiresAt(null);
      localStorage.removeItem("accessToken");
      localStorage.removeItem("refreshToken");
      localStorage.removeItem("tokenExpiresAt");
      localStorage.removeItem("user");
    }
  }, []);

  const refreshToken = useCallback(async () => {
    try {
      const storedRefreshToken =
        refreshTokenValue || localStorage.getItem("refreshToken");

      if (!storedRefreshToken) {
        throw new Error("No refresh token available");
      }

      const result = await authService.refreshToken(storedRefreshToken);

      setAccessToken(result.accessToken);
      setRefreshTokenValue(result.refreshToken);
      setTokenExpiresAt(result.expiresAt);

      localStorage.setItem("accessToken", result.accessToken);
      localStorage.setItem("refreshToken", result.refreshToken);
      localStorage.setItem("tokenExpiresAt", result.expiresAt);
    } catch (err) {
      console.error("Token refresh failed:", err);
      // If refresh fails, logout user
      await logout();
      throw err;
    }
  }, [refreshTokenValue, logout]);

  const getCurrentUser = useCallback(async () => {
    try {
      const userInfo = await authService.getCurrentUser();
      setUser(userInfo);
      localStorage.setItem("user", JSON.stringify(userInfo));
      return userInfo;
    } catch (err) {
      console.error("Get current user error:", err);
      throw err;
    }
  }, []);

  const changePassword = useCallback(
    async (currentPassword: string, newPassword: string) => {
      try {
        await authService.changePassword(currentPassword, newPassword);
        // After password change, backend logs out the user (removes cookies)
        await logout();
      } catch (err) {
        const errorMessage =
          err instanceof Error ? err.message : "Password change failed";
        setError(errorMessage);
        throw err;
      }
    },
    [logout]
  );

  const getSessions = useCallback(async () => {
    try {
      return await authService.getSessions();
    } catch (err) {
      console.error("Get sessions error:", err);
      throw err;
    }
  }, []);

  const revokeAllTokens = useCallback(async () => {
    try {
      await authService.revokeAllTokens();
      // After revoking all tokens, logout
      await logout();
    } catch (err) {
      console.error("Revoke all tokens error:", err);
      throw err;
    }
  }, [logout]);

  // Initialize auth state from localStorage
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        const storedAccessToken = localStorage.getItem("accessToken");
        const storedUser = localStorage.getItem("user");
        const storedRefreshToken = localStorage.getItem("refreshToken");

        if (storedAccessToken && storedUser) {
          // Validate the token first before setting state
          try {
            const validationResult = await authService.validateToken();

            if (validationResult.isValid) {
              // Token is valid, restore state
              setAccessToken(storedAccessToken);
              setRefreshTokenValue(storedRefreshToken);
              setUser(JSON.parse(storedUser));
            } else {
              // Token invalid, clear everything
              throw new Error("Token validation failed");
            }
          } catch (err) {
            console.log("Token validation failed, attempting refresh...");
            // Token invalid, try to refresh
            if (storedRefreshToken) {
              try {
                const result =
                  await authService.refreshToken(storedRefreshToken);
                setAccessToken(result.accessToken);
                setRefreshTokenValue(result.refreshToken);
                setTokenExpiresAt(result.expiresAt);
                localStorage.setItem("accessToken", result.accessToken);
                localStorage.setItem("refreshToken", result.refreshToken);
                localStorage.setItem("tokenExpiresAt", result.expiresAt);

                // Get user info after successful refresh
                try {
                  const userInfo = await authService.getCurrentUser();
                  setUser(userInfo);
                  localStorage.setItem("user", JSON.stringify(userInfo));
                } catch (userErr) {
                  console.error(
                    "Failed to get user info after refresh:",
                    userErr
                  );
                  throw userErr;
                }
              } catch (refreshErr) {
                console.log("Token refresh failed, clearing auth state");
                // Refresh failed, clear everything
                setUser(null);
                setAccessToken(null);
                setRefreshTokenValue(null);
                setTokenExpiresAt(null);
                localStorage.removeItem("accessToken");
                localStorage.removeItem("refreshToken");
                localStorage.removeItem("tokenExpiresAt");
                localStorage.removeItem("user");
              }
            } else {
              // No refresh token, clear everything
              setUser(null);
              setAccessToken(null);
              setRefreshTokenValue(null);
              setTokenExpiresAt(null);
              localStorage.removeItem("accessToken");
              localStorage.removeItem("refreshToken");
              localStorage.removeItem("tokenExpiresAt");
              localStorage.removeItem("user");
            }
          }
        }
      } catch (err) {
        console.error("Auth initialization error:", err);
        // Clear everything on any error
        setUser(null);
        setAccessToken(null);
        setRefreshTokenValue(null);
        setTokenExpiresAt(null);
        localStorage.removeItem("accessToken");
        localStorage.removeItem("refreshToken");
        localStorage.removeItem("tokenExpiresAt");
        localStorage.removeItem("user");
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []);

  // Auto-refresh token before it expires based on backend's expiresAt
  useEffect(() => {
    if (!accessToken || !tokenExpiresAt) return;

    const expirationTime = new Date(tokenExpiresAt).getTime();
    const currentTime = Date.now();
    const timeUntilExpiration = expirationTime - currentTime;

    // Refresh 5 minutes (300000ms) before expiration
    const refreshBeforeExpiration = 5 * 60 * 1000;
    const timeUntilRefresh = timeUntilExpiration - refreshBeforeExpiration;

    // If token expires in less than 5 minutes, refresh immediately
    if (timeUntilRefresh <= 0) {
      refreshToken().catch(console.error);
      return;
    }

    // Schedule refresh before expiration
    const timeoutId = setTimeout(() => {
      refreshToken().catch(console.error);
    }, timeUntilRefresh);

    return () => clearTimeout(timeoutId);
  }, [accessToken, tokenExpiresAt, refreshToken]);

  const value: AuthContextType = {
    user,
    isAuthenticated: !!user,
    isLoading,
    error,
    login,
    register,
    logout,
    refreshToken,
    clearError,
    getCurrentUser,
    changePassword,
    getSessions,
    revokeAllTokens,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
};

export default AuthContext;
