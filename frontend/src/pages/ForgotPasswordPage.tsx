import { useCallback, useState } from "react";
import { Box, Button, Divider, Link, Stack, Typography } from "@mui/material";
import { alpha } from "@mui/material/styles";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import AuthLayout from "../layouts/AuthLayout";
import {
  InputEmail,
  emailSchema as emailFieldSchema,
} from "../components/inputs/InputEmail";

const forgotPasswordSchema = z.object({
  email: emailFieldSchema,
});

type ForgotPasswordData = z.infer<typeof forgotPasswordSchema>;

export default function ForgotPasswordPage() {
  const navigate = useNavigate();
  const [submitted, setSubmitted] = useState(false);

  const { handleSubmit, control, reset } = useForm<ForgotPasswordData>({
    resolver: zodResolver(forgotPasswordSchema),
    mode: "onSubmit",
  });

  const handleLoginRedirect = useCallback(() => {
    navigate("/login");
  }, [navigate]);

  const handleRegisterRedirect = useCallback(() => {
    navigate("/register");
  }, [navigate]);

  const onSubmit = (data: ForgotPasswordData) => {
    setSubmitted(true);
    reset({ email: data.email });
  };

  return (
    <AuthLayout title="DIET ZYNZI">
      <Box
        component="form"
        onSubmit={handleSubmit(onSubmit)}
        noValidate
        sx={{ width: "100%", maxWidth: 400, mx: "auto" }}
      >
        <Stack spacing={2}>
          <Typography
            variant="h6"
            align="center"
            sx={{ color: (theme) => alpha(theme.palette.common.white, 0.95) }}
          >
            Przypomnienie hasła
          </Typography>
          <Typography
            variant="body2"
            align="center"
            sx={{ color: (theme) => alpha(theme.palette.common.white, 0.8) }}
          >
            Podaj adres e-mail powiązany z kontem. Jeśli istnieje w naszej bazie,
            wyślemy instrukcje ustawienia nowego hasła.
          </Typography>

          <InputEmail control={control} placeholder="Adres e-mail" />

          <Button
            type="submit"
            variant="contained"
            size="large"
            sx={{
              mt: 1,
              py: 1,
              width: "70%",
              alignSelf: "center",
              borderRadius: "10px",
              textTransform: "none",
              fontWeight: 700,
            }}
          >
            Wyślij link resetujący
          </Button>

          {submitted && (
            <Typography
              variant="body2"
              align="center"
              sx={{ color: (theme) => alpha(theme.palette.common.white, 0.9) }}
            >
              Jeżeli konto istnieje, wiadomość z instrukcją została wysłana.
            </Typography>
          )}

          <Divider
            sx={{
              my: 1,
              borderColor: (theme) => alpha(theme.palette.common.white, 0.15),
            }}
          />

          <Typography
            variant="body2"
            align="center"
            sx={{ color: (theme) => alpha(theme.palette.common.white, 0.85) }}
          >
            Pamiętasz już hasło?{" "}
            <Link
              component="button"
              type="button"
              underline="hover"
              onClick={handleLoginRedirect}
              sx={{
                color: (theme) => alpha(theme.palette.common.white, 0.95),
                fontWeight: 600,
              }}
            >
              Zaloguj się
            </Link>
          </Typography>

          <Typography
            variant="body2"
            align="center"
            sx={{ color: (theme) => alpha(theme.palette.common.white, 0.85) }}
          >
            Nie masz jeszcze konta?{" "}
            <Link
              component="button"
              type="button"
              underline="hover"
              onClick={handleRegisterRedirect}
              sx={{
                color: (theme) => alpha(theme.palette.common.white, 0.95),
                fontWeight: 600,
              }}
            >
              Zarejestruj się
            </Link>
          </Typography>
        </Stack>
      </Box>
    </AuthLayout>
  );
}
