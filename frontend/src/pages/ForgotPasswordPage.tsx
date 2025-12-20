import { useCallback, useState } from "react";
import { useNavigate } from "react-router-dom";

import AuthLayout from "../layouts/AuthLayout";
import { ForgotPasswordForm } from "../components/forms/ForgotPasswordForm/ForgotPasswordForm";
import authService from "../services/authService";

export default function ForgotPasswordPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const handleLoginRedirect = useCallback(() => {
    navigate("/login");
  }, [navigate]);

  const handleRegisterRedirect = useCallback(() => {
    navigate("/register");
  }, [navigate]);

  const handleSubmit = useCallback(
    async (data: { email: string }) => {
      setLoading(true);
      setStatusMessage(null);
      setErrorMessage(null);

      try {
        const response = await authService.forgotPassword(data.email);
        setStatusMessage(
          response?.message ||
            "Jeżeli konto istnieje, wiadomość z instrukcją została wysłana."
        );
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "Nie udało się wysłać instrukcji resetu hasła.";
        setErrorMessage(message);
      } finally {
        setLoading(false);
      }
    },
    []
  );

  return (
    <AuthLayout title="DIET ZYNZI">
      <ForgotPasswordForm
        onLoginRedirect={handleLoginRedirect}
        onRegisterRedirect={handleRegisterRedirect}
        onSubmit={handleSubmit}
        loading={loading}
        statusMessage={statusMessage}
        errorMessage={errorMessage}
      />
    </AuthLayout>
  );
}
