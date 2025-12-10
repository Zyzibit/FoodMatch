import { useState } from "react";
import {
  Box,
  Button,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";

interface ChangePasswordFormProps {
  onSubmit?: (currentPassword: string, newPassword: string) => Promise<void>;
}

export function ChangePasswordForm({ onSubmit }: ChangePasswordFormProps) {
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");

  const handleSubmit = async () => {
    if (!currentPassword || !newPassword || !confirmPassword) {
      alert("Wszystkie pola hasła muszą być wypełnione");
      return;
    }

    if (newPassword !== confirmPassword) {
      alert("Nowe hasło i potwierdzenie hasła nie są takie same");
      return;
    }

    if (newPassword.length < 6) {
      alert("Nowe hasło musi mieć co najmniej 6 znaków");
      return;
    }

    try {
      await onSubmit?.(currentPassword, newPassword);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmPassword("");
      alert("Hasło zostało zmienione pomyślnie. Zostaniesz wylogowany.");
      window.location.href = "/login";
    } catch (error) {
      console.error("Failed to change password:", error);
      alert("Błąd podczas zmiany hasła. Sprawdź aktualne hasło.");
    }
  };

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Zmiana hasła
      </Typography>
      <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
        Po zmianie hasła zostaniesz automatycznie wylogowany ze wszystkich
        urządzeń.
      </Typography>
      <Stack spacing={2}>
        <TextField
          label="Aktualne hasło"
          type="password"
          value={currentPassword}
          onChange={(e) => setCurrentPassword(e.target.value)}
          fullWidth
        />
        <TextField
          label="Nowe hasło"
          type="password"
          value={newPassword}
          onChange={(e) => setNewPassword(e.target.value)}
          fullWidth
        />
        <TextField
          label="Potwierdź nowe hasło"
          type="password"
          value={confirmPassword}
          onChange={(e) => setConfirmPassword(e.target.value)}
          fullWidth
          error={confirmPassword !== "" && newPassword !== confirmPassword}
          helperText={
            confirmPassword !== "" && newPassword !== confirmPassword
              ? "Hasła nie są takie same"
              : ""
          }
        />
        <Box sx={{ display: "flex", justifyContent: "flex-end" }}>
          <Button
            variant="contained"
            onClick={handleSubmit}
            disabled={
              !currentPassword ||
              !newPassword ||
              !confirmPassword ||
              newPassword !== confirmPassword
            }
          >
            Zmień hasło
          </Button>
        </Box>
      </Stack>
    </Paper>
  );
}
