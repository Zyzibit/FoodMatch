import { useEffect, useState } from "react";
import {
  Alert,
  Box,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogContentText,
  DialogTitle,
  List,
  ListItem,
  ListItemText,
  Paper,
  Stack,
  TextField,
  Typography,
} from "@mui/material";
import authService from "../../services/authService";
import userService from "../../services/userService";
import type { UserSession } from "../../types/auth";
import { useAuth } from "../../contexts/AuthContext";
import ExitToAppIcon from "@mui/icons-material/ExitToApp";

export default function UserAccountSettings() {
  const { user: authUser } = useAuth();
  const [userName, setUserName] = useState("");
  const [newUserName, setNewUserName] = useState("");
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [sessions, setSessions] = useState<UserSession[]>([]);
  const [openLogoutDialog, setOpenLogoutDialog] = useState(false);

  useEffect(() => {
    const loadUserProfile = async () => {
      try {
        if (authUser) {
          setUserName(authUser.username);
          setNewUserName(authUser.username);
        }
      } catch (error) {
        console.error("Failed to load user profile:", error);
      }
    };

    const loadSessions = async () => {
      try {
        const sessionData = await authService.getSessions();
        setSessions(sessionData);
      } catch (error) {
        console.error("Failed to load sessions:", error);
        setSessions([]);
      }
    };

    void loadUserProfile();
    void loadSessions();
  }, [authUser]);

  const handleChangeUserName = async () => {
    if (!newUserName || newUserName.trim() === "") {
      alert("Nazwa użytkownika nie może być pusta");
      return;
    }

    try {
      await userService.updateCurrentUserProfile({ name: newUserName });
      setUserName(newUserName);
      alert("Nazwa użytkownika została zmieniona");
    } catch (error) {
      console.error("Failed to change username:", error);
      alert("Błąd podczas zmiany nazwy użytkownika");
    }
  };

  const handleChangePassword = async () => {
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
      await authService.changePassword(currentPassword, newPassword);
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

  const handleLogoutAllSessions = async () => {
    try {
      await authService.revokeAllTokens();
      alert("Wszystkie sesje zostały wylogowane");
      window.location.href = "/login";
    } catch (error) {
      console.error("Failed to logout all sessions:", error);
      alert("Błąd podczas wylogowywania wszystkich sesji");
    }
  };

  return (
    <Stack spacing={4} sx={{ width: "100%" }}>
      <Box>
        <Typography variant="h5" fontWeight={800} gutterBottom>
          Zarządzanie kontem
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Zmień nazwę użytkownika, hasło oraz zarządzaj aktywnymi sesjami
        </Typography>
      </Box>

      {/* Change Username */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Zmiana nazwy użytkownika
        </Typography>
        <Stack spacing={2} sx={{ mt: 2 }}>
          <TextField
            label="Obecna nazwa użytkownika"
            value={userName}
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
              onClick={handleChangeUserName}
              disabled={!newUserName || newUserName === userName}
            >
              Zmień nazwę
            </Button>
          </Box>
        </Stack>
      </Paper>

      {/* Change Password */}
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
              onClick={handleChangePassword}
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

      {/* Active Sessions */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Aktywne sesje
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Lista wszystkich urządzeń, na których jesteś zalogowany.
        </Typography>
        {sessions.length === 0 ? (
          <Alert severity="info">Brak aktywnych sesji</Alert>
        ) : (
          <List>
            {sessions.map((session, index) => (
              <ListItem
                key={index}
                sx={{
                  border: (theme) => `1px solid ${theme.palette.divider}`,
                  borderRadius: 1,
                  mb: 1,
                  bgcolor: session.isCurrent
                    ? "action.selected"
                    : "background.paper",
                }}
              >
                <ListItemText
                  primary={
                    <Stack direction="row" spacing={1} alignItems="center">
                      <Typography variant="body1">
                        {session.userAgent || "Nieznane urządzenie"}
                      </Typography>
                      {session.isCurrent && (
                        <Typography
                          variant="caption"
                          sx={{
                            bgcolor: "primary.main",
                            color: "primary.contrastText",
                            px: 1,
                            py: 0.5,
                            borderRadius: 1,
                          }}
                        >
                          Bieżąca sesja
                        </Typography>
                      )}
                    </Stack>
                  }
                  secondary={
                    <>
                      <Typography variant="body2" color="text.secondary">
                        IP: {session.ipAddress}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Utworzono:{" "}
                        {new Date(session.createdAt).toLocaleString("pl-PL")}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        Wygasa:{" "}
                        {new Date(session.expiresAt).toLocaleString("pl-PL")}
                      </Typography>
                    </>
                  }
                />
              </ListItem>
            ))}
          </List>
        )}
      </Paper>

      {/* Logout All Sessions */}
      <Paper sx={{ p: 3 }}>
        <Typography variant="h6" gutterBottom>
          Zarządzanie sesjami
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Wyloguj się ze wszystkich urządzeń jednocześnie. Będziesz musiał się
          zalogować ponownie.
        </Typography>
        <Button
          variant="contained"
          color="error"
          startIcon={<ExitToAppIcon />}
          onClick={() => setOpenLogoutDialog(true)}
        >
          Wyloguj wszystkie sesje
        </Button>
      </Paper>

      {/* Confirmation Dialog */}
      <Dialog
        open={openLogoutDialog}
        onClose={() => setOpenLogoutDialog(false)}
      >
        <DialogTitle>Wyloguj wszystkie sesje?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            Czy na pewno chcesz wylogować się ze wszystkich urządzeń? Ta akcja
            zakończy wszystkie aktywne sesje i będziesz musiał się zalogować
            ponownie.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setOpenLogoutDialog(false)}>Anuluj</Button>
          <Button
            onClick={() => {
              setOpenLogoutDialog(false);
              handleLogoutAllSessions();
            }}
            color="error"
            variant="contained"
          >
            Wyloguj wszystkie
          </Button>
        </DialogActions>
      </Dialog>
    </Stack>
  );
}
