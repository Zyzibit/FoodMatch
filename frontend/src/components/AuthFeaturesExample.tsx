import { useState } from "react";
import { useAuth } from "../contexts/AuthContext";
import type { UserSession } from "../types/auth";
import {
  Box,
  Button,
  TextField,
  Typography,
  List,
  ListItem,
  ListItemText,
  Card,
  CardContent,
  Stack,
  Alert,
} from "@mui/material";

/**
 * Przykładowy komponent pokazujący użycie wszystkich endpointów autoryzacji
 * Możesz użyć tego jako referencję lub zintegrować z istniejącymi komponentami
 */
export function AuthFeaturesExample() {
  const {
    user,
    getCurrentUser,
    changePassword,
    getSessions,
    revokeAllTokens,
    logout,
  } = useAuth();

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [sessions, setSessions] = useState<UserSession[]>([]);
  const [message, setMessage] = useState<{
    type: "success" | "error";
    text: string;
  } | null>(null);

  const handleRefreshUserData = async () => {
    try {
      await getCurrentUser();
      setMessage({ type: "success", text: "Dane użytkownika odświeżone" });
    } catch (err) {
      setMessage({
        type: "error",
        text: "Błąd podczas odświeżania danych użytkownika",
      });
    }
  };

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await changePassword(currentPassword, newPassword);
      setMessage({
        type: "success",
        text: "Hasło zmienione. Zostaniesz wylogowany.",
      });
      setCurrentPassword("");
      setNewPassword("");
    } catch (err) {
      setMessage({
        type: "error",
        text: err instanceof Error ? err.message : "Błąd podczas zmiany hasła",
      });
    }
  };

  const handleGetSessions = async () => {
    try {
      const userSessions = await getSessions();
      setSessions(userSessions);
      setMessage({
        type: "success",
        text: `Znaleziono ${userSessions.length} aktywnych sesji`,
      });
    } catch (err) {
      setMessage({
        type: "error",
        text: "Błąd podczas pobierania sesji",
      });
    }
  };

  const handleRevokeAllTokens = async () => {
    if (
      confirm(
        "Czy na pewno chcesz wylogować wszystkie urządzenia? Zostaniesz wylogowany również z tego urządzenia."
      )
    ) {
      try {
        await revokeAllTokens();
        setMessage({
          type: "success",
          text: "Wszystkie tokeny zostały unieważnione",
        });
      } catch (err) {
        setMessage({
          type: "error",
          text: "Błąd podczas unieważniania tokenów",
        });
      }
    }
  };

  return (
    <Box sx={{ maxWidth: 800, mx: "auto", p: 3 }}>
      <Typography variant="h4" gutterBottom>
        Funkcje autoryzacji
      </Typography>

      {message && (
        <Alert
          severity={message.type}
          onClose={() => setMessage(null)}
          sx={{ mb: 2 }}
        >
          {message.text}
        </Alert>
      )}

      {/* User Info Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Informacje o użytkowniku
          </Typography>
          {user && (
            <Stack spacing={1}>
              <Typography>ID: {user.userId}</Typography>
              <Typography>Username: {user.username}</Typography>
              <Typography>Email: {user.email}</Typography>
              {user.roles && user.roles.length > 0 && (
                <Typography>Role: {user.roles.join(", ")}</Typography>
              )}
            </Stack>
          )}
          <Button
            variant="outlined"
            onClick={handleRefreshUserData}
            sx={{ mt: 2 }}
          >
            Odśwież dane użytkownika
          </Button>
        </CardContent>
      </Card>

      {/* Change Password Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Zmiana hasła
          </Typography>
          <Box component="form" onSubmit={handleChangePassword}>
            <Stack spacing={2}>
              <TextField
                type="password"
                label="Obecne hasło"
                value={currentPassword}
                onChange={(e) => setCurrentPassword(e.target.value)}
                required
                fullWidth
              />
              <TextField
                type="password"
                label="Nowe hasło"
                value={newPassword}
                onChange={(e) => setNewPassword(e.target.value)}
                required
                fullWidth
              />
              <Button type="submit" variant="contained">
                Zmień hasło
              </Button>
            </Stack>
          </Box>
        </CardContent>
      </Card>

      {/* Sessions Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Aktywne sesje
          </Typography>
          <Button variant="outlined" onClick={handleGetSessions} sx={{ mb: 2 }}>
            Pokaż aktywne sesje
          </Button>
          {sessions.length > 0 && (
            <List>
              {sessions.map((session, index) => (
                <ListItem
                  key={index}
                  sx={{
                    border: 1,
                    borderColor: "divider",
                    borderRadius: 1,
                    mb: 1,
                    bgcolor: session.isCurrentDevice
                      ? "action.selected"
                      : "background.paper",
                  }}
                >
                  <ListItemText
                    primary={`${session.userAgent} ${session.isCurrentDevice ? "(Obecne urządzenie)" : ""}`}
                    secondary={
                      <>
                        <Typography component="span" variant="body2">
                          IP: {session.ipAddress}
                        </Typography>
                        <br />
                        <Typography component="span" variant="body2">
                          Ostatnie użycie:{" "}
                          {new Date(session.lastUsed).toLocaleString()}
                        </Typography>
                      </>
                    }
                  />
                </ListItem>
              ))}
            </List>
          )}
        </CardContent>
      </Card>

      {/* Revoke Tokens Section */}
      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="h6" gutterBottom>
            Zarządzanie sesjami
          </Typography>
          <Stack spacing={2}>
            <Button
              variant="outlined"
              color="error"
              onClick={handleRevokeAllTokens}
            >
              Wyloguj wszystkie urządzenia
            </Button>
            <Button variant="outlined" color="warning" onClick={logout}>
              Wyloguj się
            </Button>
          </Stack>
        </CardContent>
      </Card>
    </Box>
  );
}

export default AuthFeaturesExample;
