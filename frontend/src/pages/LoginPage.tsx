import AuthLayout from "../layouts/AuthLayout";
import { LoginForm } from "../components/forms/LoginForm/LoginForm";

export default function LoginPage() {
  return (
    <AuthLayout title="DIET ZYNZI">
      <LoginForm />
    </AuthLayout>
  );
}
