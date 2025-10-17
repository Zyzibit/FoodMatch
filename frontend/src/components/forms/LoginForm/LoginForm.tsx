import { Box, Paper, Stack, Typography, Button, Link } from "@mui/material";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import { InputLogin } from "../../inputs/InputLogin";
import { InputPassword, passwordSchema } from "../../inputs/InputPassword";

// ✅ Walidacja logowania
const loginSchema = z.object({
  login: z
    .string()
    .min(3, "Login musi mieć co najmniej 3 znaki")
    .max(20, "Login może mieć maksymalnie 20 znaków")
    .regex(/^[a-zA-Z0-9._-]+$/, "Dozwolone: litery, cyfry, ., _, -"),
  password: passwordSchema, // min 8, 1 wielka litera, 1 cyfra
});

export type LoginFormData = z.infer<typeof loginSchema>;

type Props = {
  onSubmitForm?: (data: LoginFormData) => void;
  onRegisterClick?: () => void;
  onForgotClick?: () => void;
  loading?: boolean;
};

export function LoginForm({
  onSubmitForm,
  onRegisterClick,
  onForgotClick,
  loading = false,
}: Props) {
  const { handleSubmit, control, reset } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    mode: "onSubmit",
  });

  const onSubmit = (data: LoginFormData) => {
    onSubmitForm?.(data);
    console.log("🔐 Logowanie:", data);
    // reset(); // jeśli chcesz czyścić po udanym logowaniu
  };

  return (
    <Box
      display="flex"
      justifyContent="center"
      alignItems="center"
      sx={{ width: "100%", minHeight: "100vh", bgcolor: "#f5f5f5" }}
    >
      <Paper
        elevation={3}
        sx={{ p: 4, width: "100%", maxWidth: 420, borderRadius: 3 }}
      >
        <Typography variant="h5" align="center" fontWeight={600} mb={3}>
          Logowanie
        </Typography>

        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack spacing={2}>
            <InputLogin control={control} placeholder="Wpisz login" />
            <InputPassword control={control} placeholder="Wpisz hasło" />

            <Box sx={{ mt: 0.5 }}>
              <Typography variant="body2" color="text.secondary">
                Nie masz konta?{" "}
                <Link
                  component="button"
                  type="button"
                  underline="hover"
                  onClick={onRegisterClick}
                >
                  Zarejestruj się
                </Link>
              </Typography>
              <Typography
                variant="body2"
                color="text.secondary"
                sx={{ mt: 0.5 }}
              >
                Nie pamiętasz hasła?{" "}
                <Link
                  component="button"
                  type="button"
                  underline="hover"
                  onClick={onForgotClick}
                >
                  Przypomnienie hasła
                </Link>
              </Typography>
            </Box>

            <Button
              type="submit"
              variant="contained"
              size="large"
              disabled={loading}
              sx={{
                mt: 2,
                borderRadius: "10px",
                textTransform: "none",
                fontWeight: 700,
                py: 1.2,
                // kolor jak na screenie (jeśli chcesz wymusić odcień)
                backgroundColor: "#3d3bff",
                "&:hover": { backgroundColor: "#3432e6" },
              }}
            >
              Zaloguj
            </Button>
          </Stack>
        </form>
      </Paper>
    </Box>
  );
}
