import { useCallback } from "react";
import { useNavigate } from "react-router-dom";

import AuthLayout from "../layouts/AuthLayout";
import { ForgotPasswordForm } from "../components/forms/ForgotPasswordForm/ForgotPasswordForm";

export default function ForgotPasswordPage() {
  const navigate = useNavigate();

  const handleLoginRedirect = useCallback(() => {
    navigate("/login");
  }, [navigate]);

  const handleRegisterRedirect = useCallback(() => {
    navigate("/register");
  }, [navigate]);

  return (
    <AuthLayout title="DIET ZYNZI">
      <ForgotPasswordForm
        onLoginRedirect={handleLoginRedirect}
        onRegisterRedirect={handleRegisterRedirect}
      />
    </AuthLayout>
  );
}
