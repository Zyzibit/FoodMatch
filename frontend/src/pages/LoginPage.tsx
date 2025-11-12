import { useCallback, type ComponentProps } from "react";
import { useNavigate } from "react-router-dom";
import AuthLayout from "../layouts/AuthLayout";
import { LoginForm } from "../components/forms/LoginForm/LoginForm";

type LoginFormSubmit = NonNullable<
  ComponentProps<typeof LoginForm>["onSubmitForm"]
>;

export default function LoginPage() {
  const navigate = useNavigate();

  const handleLogin = useCallback<LoginFormSubmit>(
    (_data) => {
      navigate("/app/plan", { replace: true });
    },
    [navigate]
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
      />
    </AuthLayout>
  );
}
