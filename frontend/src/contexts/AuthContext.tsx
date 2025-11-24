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

  const clearError = useCallback(() => {
    setError(null);
  }, []);

  const login = useCallback(async (username: string, password: string) => {
    try {
      setIsLoading(true);
      setError(null);

      const result = await authService.login(username, password);

      // Backend sets tokens in httpOnly cookies
      setUser(result.user);
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

        // Backend sets tokens in httpOnly cookies
        setUser(result.user);
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
      // Call backend to invalidate cookies
      await authService.logout();
      setUser(null);
    } catch (err) {
      console.error("Logout error:", err);
      // Even if backend call fails, ensure local state is cleared
      setUser(null);
    }
  }, []);

  const refreshToken = useCallback(async () => {
    try {
      // Backend uses refresh token from httpOnly cookie
      await authService.refreshToken();
    } catch (err) {
      console.error("Token refresh failed:", err);
      // If refresh fails, logout user
      await logout();
      throw err;
    }
  }, [logout]);

  const getCurrentUser = useCallback(async () => {
    try {
      const userInfo = await authService.getCurrentUser();
      setUser(userInfo);
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

  // Initialize auth state by checking session with backend
  useEffect(() => {
    const initializeAuth = async () => {
      try {
        // Try to get current user - backend will validate session from httpOnly cookie
        const userInfo = await authService.getCurrentUser();
        setUser(userInfo);
      } catch (err) {
        // No valid session - user needs to login
        console.log("No active session");
        setUser(null);
      } finally {
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []);

  // Note: Token refresh is handled automatically by backend middleware (TokenRefreshMiddleware)

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
