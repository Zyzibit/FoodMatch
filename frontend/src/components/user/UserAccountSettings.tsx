import { ChangeEvent, useEffect, useRef, useState } from "react";
import { Avatar, Box, Button, Divider, Stack, TextField, Typography } from "@mui/material";

const LOCAL_STORAGE_KEY = "user_account_settings";

type StoredSettings = {
  username: string;
  avatar?: string | null;
};

const defaultSettings: StoredSettings = {
  username: "Jan Kowalski",
  avatar: null,
};

const deviceSessions = [
  { id: "device-1", name: "Chrome · Windows", location: "Warszawa, PL", lastActive: "dzisiaj, 10:24" },
  { id: "device-2", name: "Safari · iOS", location: "Gdańsk, PL", lastActive: "wczoraj, 21:17" },
];

const buildPayload = (
  state: { username: string; avatar: string | null },
  override?: Partial<StoredSettings>
): StoredSettings => ({
  username: override?.username ?? state.username,
  avatar: override?.avatar ?? state.avatar,
});

export default function UserAccountSettings() {
  const fileInputRef = useRef<HTMLInputElement | null>(null);
  const [username, setUsername] = useState(defaultSettings.username);
  const [avatar, setAvatar] = useState<string | null>(defaultSettings.avatar ?? null);
  const [passwords, setPasswords] = useState({ current: "", next: "", confirm: "" });
  const avatarInitial = username.trim().slice(0, 1).toUpperCase() || "?";

  useEffect(() => {
    try {
      const raw = localStorage.getItem(LOCAL_STORAGE_KEY);
      if (!raw) return;
      const parsed: StoredSettings = JSON.parse(raw);
      if (parsed.username) setUsername(parsed.username);
      if (typeof parsed.avatar === "string" || parsed.avatar === null) {
        setAvatar(parsed.avatar);
      }
    } catch (error) {
      console.warn("Nie udało się wczytać ustawień profilu", error);
    }
  }, []);

  const persist = (override?: Partial<StoredSettings>) => {
    const payload = buildPayload(
      {
        username,
        avatar,
      },
      override
    );
    localStorage.setItem(LOCAL_STORAGE_KEY, JSON.stringify(payload));
  };

  const handleUsernameSave = () => {
    const trimmed = username.trim();
    if (trimmed.length < 3) {
      alert("Nazwa użytkownika musi mieć co najmniej 3 znaki.");
      return;
    }
    setUsername(trimmed);
    persist({ username: trimmed });
    alert("Nazwa użytkownika została zaktualizowana.");
  };

  const handleAvatarButtonClick = () => {
    fileInputRef.current?.click();
  };

  const handleAvatarChange = (event: ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;
    if (!file.type.startsWith("image/")) {
      alert("Wybierz plik graficzny (PNG, JPG, WEBP).");
      return;
    }
    if (file.size > 2 * 1024 * 1024) {
      alert("Zdjęcie nie może być większe niż 2 MB.");
      return;
    }

    const reader = new FileReader();
    reader.onload = () => {
      const result = typeof reader.result === "string" ? reader.result : null;
      setAvatar(result);
      persist({ avatar: result });
      alert("Zdjęcie profilowe zostało zapisane lokalnie.");
    };
    reader.readAsDataURL(file);
    event.target.value = "";
  };

  const handleAvatarReset = () => {
    setAvatar(null);
    persist({ avatar: null });
  };

  const handlePasswordField = (field: keyof typeof passwords, value: string) => {
    setPasswords((prev) => ({ ...prev, [field]: value }));
  };

  const handlePasswordChange = () => {
    if (!passwords.current || !passwords.next || !passwords.confirm) {
      alert("Uzupełnij wszystkie pola, aby zmienić hasło.");
      return;
    }
    if (passwords.next.length < 8) {
      alert("Nowe hasło powinno mieć co najmniej 8 znaków.");
      return;
    }
    if (passwords.next !== passwords.confirm) {
      alert("Nowe hasło i potwierdzenie muszą być takie same.");
      return;
    }
    if (passwords.current === passwords.next) {
      alert("Nowe hasło musi różnić się od obecnego.");
      return;
    }
    setPasswords({ current: "", next: "", confirm: "" });
    alert("Hasło zostałoby zmienione po podpięciu backendu.");
  };

  const handleEndSession = (sessionId: string) => {
    alert(`Zakończono sesję: ${sessionId}. Funkcja zostanie podpięta do API.`);
  };

  return (
    <Stack spacing={4} sx={{ width: "100%" }}>
      <Box>
        <Typography variant="h5" fontWeight={800} gutterBottom>
          Profil użytkownika
        </Typography>
        <Typography variant="body2" color="text.secondary">
          Zarządzaj danymi konta, wyglądem profilu oraz preferencjami bezpieczeństwa.
          Wszystkie zmiany są zapisywane lokalnie do czasu podpięcia backendu.
        </Typography>
      </Box>

      <Stack spacing={3}>
        <Box>
          <Typography variant="subtitle1" fontWeight={700} gutterBottom>
            Zdjęcie i nazwa
          </Typography>
          <Stack
            direction={{ xs: "column", sm: "row" }}
            spacing={3}
            alignItems={{ sm: "center" }}
          >
            <Avatar src={avatar ?? undefined} sx={{ width: 96, height: 96, fontSize: 32 }}>
              {avatarInitial}
            </Avatar>
            <Stack spacing={1} sx={{ flex: 1 }}>
              <Stack direction="row" spacing={2}>
                <Button variant="outlined" onClick={handleAvatarButtonClick}>
                  Zmień zdjęcie
                </Button>
                {avatar && (
                  <Button variant="text" color="error" onClick={handleAvatarReset}>
                    Usuń zdjęcie
                  </Button>
                )}
              </Stack>
              <Typography variant="caption" color="text.secondary">
                Obsługujemy pliki PNG, JPG oraz WEBP do 2 MB. Zdjęcie zostanie zapisane lokalnie.
              </Typography>
            </Stack>
            <input
              ref={fileInputRef}
              type="file"
              accept="image/*"
              hidden
              onChange={handleAvatarChange}
            />
          </Stack>
        </Box>

        <Stack direction={{ xs: "column", sm: "row" }} spacing={2} alignItems="flex-start">
          <TextField
            label="Nazwa użytkownika"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            fullWidth
          />
          <Button variant="contained" onClick={handleUsernameSave} sx={{ alignSelf: "stretch" }}>
            Zapisz nazwę
          </Button>
        </Stack>
      </Stack>

      <Divider />

      <Box>
        <Typography variant="subtitle1" fontWeight={700} gutterBottom>
          Zmiana hasła
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Po podpięciu API zweryfikujemy bieżące hasło i ustawimy nowe na serwerze.
          Na razie formularz demonstruje walidację i zachowany przepływ.
        </Typography>
        <Stack spacing={2}>
          <TextField
            label="Aktualne hasło"
            type="password"
            value={passwords.current}
            onChange={(e) => handlePasswordField("current", e.target.value)}
            fullWidth
          />
          <TextField
            label="Nowe hasło"
            type="password"
            value={passwords.next}
            onChange={(e) => handlePasswordField("next", e.target.value)}
            fullWidth
          />
          <TextField
            label="Powtórz nowe hasło"
            type="password"
            value={passwords.confirm}
            onChange={(e) => handlePasswordField("confirm", e.target.value)}
            fullWidth
          />
          <Box sx={{ display: "flex", justifyContent: "flex-end" }}>
            <Button variant="contained" onClick={handlePasswordChange}>
              Ustaw nowe hasło
            </Button>
          </Box>
        </Stack>
      </Box>

      <Divider />

      <Box>
        <Typography variant="subtitle1" fontWeight={700} gutterBottom>
          Aktywne sesje
        </Typography>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          Monitoruj zalogowane urządzenia. Funkcja kończenia sesji zostanie powiązana z backendem.
        </Typography>
        <Stack spacing={1.5}>
          {deviceSessions.map((session) => (
            <Box
              key={session.id}
              sx={{
                display: "flex",
                flexDirection: { xs: "column", sm: "row" },
                justifyContent: "space-between",
                gap: 1,
                p: 1.5,
                borderRadius: 2,
                border: (theme) => `1px solid ${theme.palette.divider}`,
              }}
            >
              <Box>
                <Typography fontWeight={600}>{session.name}</Typography>
                <Typography variant="body2" color="text.secondary">
                  {session.location} · {session.lastActive}
                </Typography>
              </Box>
              <Button
                size="small"
                color="error"
                onClick={() => handleEndSession(session.id)}
                sx={{ alignSelf: { xs: "flex-start", sm: "center" } }}
              >
                Wyloguj urządzenie
              </Button>
            </Box>
          ))}
        </Stack>
      </Box>
    </Stack>
  );
}
