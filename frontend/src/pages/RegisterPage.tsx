import { useCallback } from "react";
import { useNavigate } from "react-router-dom";
import AuthLayout from "../layouts/AuthLayout";
import { RegisterForm } from "../components/forms/RegisterForm/RegisterForm";

export default function RegisterPage() {
  const navigate = useNavigate();

  const handleLoginRedirect = useCallback(() => {
    navigate("/login");
  }, [navigate]);

  return (
    <AuthLayout title="DIET ZYNZI">
      <RegisterForm onLoginClick={handleLoginRedirect} />
    </AuthLayout>
  );
}
