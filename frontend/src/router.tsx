import { createBrowserRouter, Navigate } from "react-router-dom";
import LoginPage from "./pages/LoginPage";
import RegisterPage from "./pages/RegisterPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import DashboardLayout from "./layouts/DashboardLayout";
import PlanPage from "./pages/PlanPage";
import ShoppingListPage from "./pages/ShoppingListPage";
import RecipesPage from "./pages/RecipesPage";
import SettingsPage from "./pages/SettingsPage";
import UserDashboardPage from "./pages/UserDashboardPage";

export const router = createBrowserRouter([
  {
    path: "/",
    element: <Navigate to="/login" replace />,
  },
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/register",
    element: <RegisterPage />,
  },
  {
    path: "/forgot-password",
    element: <ForgotPasswordPage />,
  },
  {
    path: "/app",
    element: <DashboardLayout />,
    children: [
      { index: true, element: <Navigate to="plan" replace /> },
      { path: "plan", element: <PlanPage /> },
      { path: "lista", element: <ShoppingListPage /> },
      { path: "przepisy", element: <RecipesPage /> },
      { path: "ustawienia", element: <SettingsPage /> },
      { path: "user", element: <UserDashboardPage /> },
    ],
  },
  {
    path: "*",
    element: <Navigate to="/login" replace />,
  },
]);
