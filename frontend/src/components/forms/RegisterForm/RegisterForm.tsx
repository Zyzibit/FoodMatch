import { Box, Button, Typography, Paper, Stack, Divider } from "@mui/material";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import { InputEmail } from "../../inputs/InputEmail";
import { InputLogin } from "../../inputs/InputLogin";
import { InputPassword, passwordSchema } from "../../inputs/InputPassword";

// 🧠 schemat walidacji rejestracji
const registerSchema = z
  .object({
    email: z
      .string()
      .min(1, "Email jest wymagany")
      .email("Niepoprawny adres email"),
    login: z
      .string()
      .min(3, "Login musi mieć co najmniej 3 znaki")
      .max(20, "Login może mieć maksymalnie 20 znaków")
      .regex(/^[a-zA-Z0-9._-]+$/, "Dozwolone: litery, cyfry, ., _, -"),
    password: passwordSchema,
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Hasła muszą być takie same",
    path: ["confirmPassword"],
  });

type RegisterFormData = z.infer<typeof registerSchema>;

export function RegisterForm() {
  const { handleSubmit, control, reset } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    mode: "onSubmit",
  });

  const onSubmit = (data: RegisterFormData) => {
    console.log("📦 Dane rejestracji:", data);
    reset();
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
        sx={{
          p: 4,
          width: "100%",
          maxWidth: 420,
          borderRadius: 3,
        }}
      >
        <Typography variant="h5" align="center" fontWeight={600} mb={3}>
          Rejestracja
        </Typography>

        <form onSubmit={handleSubmit(onSubmit)} noValidate>
          <Stack spacing={2}>
            <InputEmail control={control} placeholder="Email" />
            <InputLogin control={control} placeholder="Login" />
            <InputPassword control={control} placeholder="Wpisz hasło" />
            <InputPassword
              name="confirmPassword"
              control={control}
              placeholder="Powtórz hasło"
            />

            <Button
              type="submit"
              variant="contained"
              size="large"
              sx={{
                mt: 1,
                borderRadius: "10px",
                textTransform: "none",
                fontWeight: 600,
              }}
            >
              Zarejestruj się
            </Button>

            <Divider sx={{ my: 2 }} />

            <Typography variant="body2" align="center" color="text.secondary">
              Masz już konto?{" "}
              <Typography
                component="span"
                color="primary"
                sx={{ cursor: "pointer", fontWeight: 500 }}
              >
                Zaloguj się
              </Typography>
            </Typography>
          </Stack>
        </form>
      </Paper>
    </Box>
  );
}
