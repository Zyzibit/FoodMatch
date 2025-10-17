import AuthLayout from "../layouts/AuthLayout";
import { RegisterForm } from "../components/forms/RegisterForm/RegisterForm";

export default function RegisterPage() {
  return (
    <AuthLayout title="DIET ZYNZI">
      <RegisterForm />
    </AuthLayout>
  );
}
