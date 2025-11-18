import { useCallback, useState, type ComponentProps } from "react";
import { useNavigate } from "react-router-dom";
import AuthLayout from "../layouts/AuthLayout";
import { LoginForm } from "../components/forms/LoginForm/LoginForm";
import { useAuth } from "../contexts/AuthContext";

type LoginFormSubmit = NonNullable<
  ComponentProps<typeof LoginForm>["onSubmitForm"]
>;

export default function LoginPage() {
  const navigate = useNavigate();
  const { login, error, clearError } = useAuth();
  const [isLoading, setIsLoading] = useState(false);

  const handleLogin = useCallback<LoginFormSubmit>(
    async (data) => {
      setIsLoading(true);
      clearError();

      try {
        await login(data.login, data.password);
        navigate("/app/plan", { replace: true });
      } catch (err) {
        console.error("Login error:", err);
        // Error is handled by AuthContext
      } finally {
        setIsLoading(false);
      }
    },
    [login, navigate, clearError]
  );

  const handleRegisterRedirect = useCallback(() => {
    navigate("/register");
  }, [navigate]);

  const handleForgotPasswordRedirect = useCallback(() => {
    navigate("/forgot-password");
  }, [navigate]);

  return (
    <AuthLayout title="DIET ZYNZI">
      <LoginForm
        onSubmitForm={handleLogin}
        onRegisterClick={handleRegisterRedirect}
        onForgotPasswordClick={handleForgotPasswordRedirect}
        loading={isLoading}
      />
      {error && (
        <div style={{ color: "red", marginTop: "1rem", textAlign: "center" }}>
          {error}
        </div>
      )}
    </AuthLayout>
  );
}
