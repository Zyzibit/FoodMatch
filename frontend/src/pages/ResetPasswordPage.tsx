import { useCallback, useState } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { Alert, Box, Button, Stack, Typography } from "@mui/material";

import AuthLayout from "../layouts/AuthLayout";
import { ResetPasswordForm } from "../components/forms/ResetPasswordForm/ResetPasswordForm";
import authService from "../services/authService";

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  const [loading, setLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

  const token = searchParams.get("token") ?? "";
  const email = searchParams.get("email") ?? "";

  const hasRequiredParams = Boolean(token && email);

  const handleLoginRedirect = useCallback(() => {
    navigate("/login");
  }, [navigate]);

  const handleResetPassword = useCallback(
    async (newPassword: string) => {
      if (!hasRequiredParams) {
        setErrorMessage(
          "Brakuje danych do resetu hasła. Poproś o nowy link resetujący."
        );
        return;
      }

      setLoading(true);
      setSuccessMessage(null);
      setErrorMessage(null);

      try {
        const response = await authService.resetPassword(
          email,
          token,
          newPassword
        );

        setSuccessMessage(
          response?.message ||
            "Hasło zostało zresetowane. Możesz zalogować się nowym hasłem."
        );
      } catch (error) {
        const message =
          error instanceof Error
            ? error.message
            : "Nie udało się zresetować hasła. Spróbuj ponownie.";
        setErrorMessage(message);
      } finally {
        setLoading(false);
      }
    },
    [email, token, hasRequiredParams]
  );

  if (!hasRequiredParams) {
    return (
      <AuthLayout title="DIET ZYNZI">
        <Box sx={{ width: "100%", maxWidth: 480, mx: "auto", mt: 2 }}>
          <Stack spacing={2} alignItems="center">
            <Alert severity="error" sx={{ width: "100%" }}>
              Nie odnaleziono danych resetu hasła. Upewnij się, że korzystasz z
              pełnego linku z wiadomości e-mail lub poproś o nowy.
            </Alert>
            <Button
              variant="contained"
              onClick={() => navigate("/forgot-password")}
            >
              Poproś o nowy link
            </Button>
            <Button variant="text" onClick={handleLoginRedirect}>
              Wróć do logowania
            </Button>
          </Stack>
        </Box>
      </AuthLayout>
    );
  }

  return (
    <AuthLayout title="DIET ZYNZI">
      <ResetPasswordForm
        email={email}
        onSubmit={handleResetPassword}
        onLoginRedirect={handleLoginRedirect}
        loading={loading}
        successMessage={successMessage}
        errorMessage={errorMessage}
      />
      {successMessage && (
        <Typography
          variant="body2"
          align="center"
          sx={{ mt: 2, color: "common.white" }}
        >
          Po ustawieniu nowego hasła możesz przejść do logowania, aby wejść na
          swoje konto.
        </Typography>
      )}
    </AuthLayout>
  );
}
