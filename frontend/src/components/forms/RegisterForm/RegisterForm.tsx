import { Box, Stack, Button, Link, Divider, Typography } from "@mui/material";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";

import {
  InputEmail,
  emailSchema as emailFieldSchema,
} from "../../inputs/InputEmail";
import {
  InputLogin,
  loginSchema as loginFieldSchema,
} from "../../inputs/InputLogin";
import { InputPassword, passwordSchema } from "../../inputs/InputPassword";
const registerSchema = z
  .object({
    email: emailFieldSchema,
    login: loginFieldSchema,
    password: passwordSchema,
    confirmPassword: z.string(),
  })
  .refine((d) => d.password === d.confirmPassword, {
    path: ["confirmPassword"],
    message: "Hasła muszą być takie same",
  });

export type RegisterFormData = z.infer<typeof registerSchema>;

export function RegisterForm({
  onSubmitForm,
  onLoginClick,
  loading = false,
}: {
  onSubmitForm?: (data: RegisterFormData) => void;
  onLoginClick?: () => void;
  loading?: boolean;
}) {
  const { handleSubmit, control } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
    mode: "onSubmit",
  });

  const onSubmit = (data: RegisterFormData) => onSubmitForm?.(data);

  return (
    <Box
      component="form"
      onSubmit={handleSubmit(onSubmit)}
      noValidate
      sx={{ width: "100%", maxWidth: 400, mx: "auto" }}
    >
      <Stack spacing={1.5}>
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
          Zarejestruj się
        </Button>

        <Divider sx={{ my: 1, borderColor: "rgba(255,255,255,0.15)" }} />

        <Typography
          variant="body2"
          align="center"
          sx={{ color: "rgba(255,255,255,0.85)", fontSize: "0.85rem" }}
        >
          Masz już konto?{" "}
          <Link
            component="button"
            type="button"
            underline="hover"
            onClick={onLoginClick}
            sx={{ color: "rgba(255,255,255,0.95)", fontWeight: 600 }}
          >
            Zaloguj się
          </Link>
        </Typography>
      </Stack>
    </Box>
  );
}
