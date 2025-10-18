import { Box, Stack, Button, Link, Divider, Typography } from "@mui/material";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import {
  InputLogin,
  loginSchema as loginFieldSchema,
} from "../../inputs/InputLogin";
import { InputPassword, passwordSchema } from "../../inputs/InputPassword";

const loginSchema = z.object({
  login: loginFieldSchema, // ✅ re-używamy schematu pola
  password: passwordSchema, // ✅ spójnie z InputPassword
});

type LoginFormData = z.infer<typeof loginSchema>;

export function LoginForm(props: {
  onSubmitForm?: (data: LoginFormData) => void;
  onRegisterClick?: () => void;
  onForgotPasswordClick?: () => void;
  loading?: boolean;
}) {
  const {
    onSubmitForm,
    onRegisterClick,
    onForgotPasswordClick,
    loading = false,
  } = props;

  const { handleSubmit, control } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    mode: "onSubmit",
  });

  const onSubmit = (data: LoginFormData) => onSubmitForm?.(data);

  return (
    <Box
      component="form"
      onSubmit={handleSubmit(onSubmit)}
      noValidate
      sx={{ width: "100%", maxWidth: 400, mx: "auto" }}
    >
      <Stack spacing={1.5}>
        <InputLogin control={control} placeholder="Wpisz login" />
        <InputPassword control={control} placeholder="Wpisz hasło" />
        <Button
          type="submit"
          variant="contained"
          size="large"
          disabled={loading}
          sx={{
            mt: 1.5,
            py: 1,
            width: "70%",
            alignSelf: "center",
            borderRadius: "10px",
            textTransform: "none",
            fontWeight: 700,
          }}
        >
          Zaloguj
        </Button>
        <Divider sx={{ my: 1, borderColor: "rgba(255,255,255,0.15)" }} />
        <Typography
          variant="body2"
          align="center"
          sx={{ color: "rgba(255,255,255,0.85)", fontSize: "0.85rem" }}
        >
          Nie masz konta?{" "}
          <Link
            component="button"
            type="button"
            underline="hover"
            onClick={onRegisterClick}
            sx={{ color: "rgba(255,255,255,0.95)", fontWeight: 600 }}
          >
            Zarejestruj się
          </Link>
        </Typography>
        <Typography
          variant="body2"
          align="center"
          sx={{ color: "rgba(255,255,255,0.85)", fontSize: "0.85rem" }}
        >
          Nie pamiętasz hasła?{" "}
          <Link
            component="button"
            type="button"
            underline="hover"
            onClick={onForgotPasswordClick}
            sx={{ color: "rgba(255,255,255,0.95)", fontWeight: 600 }}
          >
            Przypomnienie hasła
          </Link>
        </Typography>
      </Stack>
    </Box>
  );
}
