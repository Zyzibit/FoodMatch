import { useCallback, useState, type ComponentProps } from "react";
import { useNavigate } from "react-router-dom";
import AuthLayout from "../layouts/AuthLayout";
import { RegisterForm } from "../components/forms/RegisterForm/RegisterForm";
import { useAuth } from "../contexts/AuthContext";

type RegisterFormSubmit = NonNullable<
  ComponentProps<typeof RegisterForm>["onSubmitForm"]
>;

export default function RegisterPage() {
  const navigate = useNavigate();
  const { register, error, clearError } = useAuth();
  const [isLoading, setIsLoading] = useState(false);

  const handleRegister = useCallback<RegisterFormSubmit>(
    async (data) => {
      setIsLoading(true);
      clearError();

      try {
        await register(data.login, data.email, data.password);
        navigate("/app/plan", { replace: true });
      } catch (err) {
        console.error("Registration error:", err);
        // Error is handled by AuthContext
      } finally {
        setIsLoading(false);
      }
    },
    [register, navigate, clearError]
  );

  const handleLoginRedirect = useCallback(() => {
    navigate("/login");
  }, [navigate]);

  return (
    <AuthLayout title="DIET ZYNZI">
      <RegisterForm
        onSubmitForm={handleRegister}
        onLoginClick={handleLoginRedirect}
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
