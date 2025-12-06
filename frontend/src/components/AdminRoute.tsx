import { Navigate } from "react-router-dom";
import { useAuth } from "../contexts/AuthContext";
import { isAdmin } from "../utils/roleUtils";
import type { ReactNode } from "react";

interface AdminRouteProps {
  children: ReactNode;
}

export default function AdminRoute({ children }: AdminRouteProps) {
  const { user, isAuthenticated } = useAuth();

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (!isAdmin(user)) {
    return <Navigate to="/app/plan" replace />;
  }

  return <>{children}</>;
}
