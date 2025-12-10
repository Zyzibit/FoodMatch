import { useState } from "react";
import {
  Box,
  Button,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";

interface ChangeUsernameFormProps {
  currentUsername: string;
  onSubmit?: (newUsername: string) => Promise<void>;
}

export function ChangeUsernameForm({
  currentUsername,
  onSubmit,
}: ChangeUsernameFormProps) {
  const [newUserName, setNewUserName] = useState(currentUsername);

  const handleSubmit = async () => {
    if (!newUserName || newUserName.trim() === "") {
      alert("Nazwa użytkownika nie może być pusta");
      return;
    }

    try {
      await onSubmit?.(newUserName);
      alert("Nazwa użytkownika została zmieniona");
    } catch (error) {
      console.error("Failed to change username:", error);
      alert("Błąd podczas zmiany nazwy użytkownika");
    }
  };

  return (
    <Paper sx={{ p: 3 }}>
      <Typography variant="h6" gutterBottom>
        Zmiana nazwy użytkownika
      </Typography>
      <Stack spacing={2} sx={{ mt: 2 }}>
        <TextField
          label="Obecna nazwa użytkownika"
          value={currentUsername}
          disabled
          fullWidth
        />
        <TextField
          label="Nowa nazwa użytkownika"
          value={newUserName}
          onChange={(e) => setNewUserName(e.target.value)}
          fullWidth
        />
        <Box sx={{ display: "flex", justifyContent: "flex-end" }}>
          <Button
            variant="contained"
            onClick={handleSubmit}
            disabled={!newUserName || newUserName === currentUsername}
          >
            Zmień nazwę
          </Button>
        </Box>
      </Stack>
    </Paper>
  );
}
