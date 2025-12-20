import {
  Alert,
  Box,
  Button,
  Divider,
  Link,
  Stack,
  Typography,
} from "@mui/material";
import { alpha } from "@mui/material/styles";
import { useForm } from "react-hook-form";
import { z } from "zod";
import { zodResolver } from "@hookform/resolvers/zod";
import { InputPassword, passwordSchema } from "../../inputs/InputPassword";

const resetPasswordSchema = z
  .object({
    password: passwordSchema,
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    path: ["confirmPassword"],
    message: "Hasła muszą być takie same",
  });

export type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

interface ResetPasswordFormProps {
  email: string;
  onSubmit?: (newPassword: string) => Promise<void> | void;
  onLoginRedirect?: () => void;
  loading?: boolean;
  successMessage?: string | null;
  errorMessage?: string | null;
}

export function ResetPasswordForm({
  email,
  onSubmit,
  onLoginRedirect,
  loading = false,
  successMessage,
  errorMessage,
}: ResetPasswordFormProps) {
  const { handleSubmit, control, reset } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
    mode: "onSubmit",
  });

  const handleFormSubmit = async (data: ResetPasswordFormData) => {
    try {
      await onSubmit?.(data.password);
      reset({ password: "", confirmPassword: "" });
    } catch (error) {
      // Error handling is delegated to the parent
    }
  };

  return (
    <Box
      component="form"
      onSubmit={handleSubmit(handleFormSubmit)}
      noValidate
      sx={{ width: "100%", maxWidth: 400, mx: "auto" }}
    >
      <Stack spacing={2}>
        <Typography
          variant="h6"
          align="center"
          sx={{ color: (theme) => alpha(theme.palette.common.white, 0.95) }}
        >
          Ustaw nowe hasło
        </Typography>
        <Typography
          variant="body2"
          align="center"
          sx={{ color: (theme) => alpha(theme.palette.common.white, 0.8) }}
        >
          Resetujesz hasło dla konta <strong>{email}</strong>. Ustal nowe hasło,
          aby odzyskać dostęp.
        </Typography>

        <InputPassword
          name="password"
          control={control}
          placeholder="Nowe hasło"
        />
        <InputPassword
          name="confirmPassword"
          control={control}
          placeholder="Powtórz nowe hasło"
        />

        <Button
          type="submit"
          variant="contained"
          size="large"
          disabled={loading}
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
          {loading ? "Zapisywanie..." : "Zresetuj hasło"}
        </Button>

        {successMessage && (
          <Alert severity="success" sx={{ mt: 1 }}>
            {successMessage}
          </Alert>
        )}

        {errorMessage && (
          <Alert severity="error" sx={{ mt: 1 }}>
            {errorMessage}
          </Alert>
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
            onClick={onLoginRedirect}
            sx={{
              color: (theme) => alpha(theme.palette.common.white, 0.95),
              fontWeight: 600,
            }}
          >
            Wróć do logowania
          </Link>
        </Typography>
      </Stack>
    </Box>
  );
}
