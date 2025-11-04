import { useEffect, useState } from "react";
import {
  Box,
  Paper,
  Typography,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
} from "@mui/material";

export default function SettingsPage() {
  const [theme, setTheme] = useState<string>("light");
  const [textSize, setTextSize] = useState<string>("md");
  const [language, setLanguage] = useState<string>("pl");

  useEffect(() => {
    try {
      const raw = localStorage.getItem("app_settings");
      if (!raw) return;
      const parsed = JSON.parse(raw);
      if (parsed.theme) setTheme(parsed.theme);
      if (parsed.textSize) setTextSize(parsed.textSize);
      if (parsed.language) setLanguage(parsed.language);
    } catch (e) {
      // ignore
    }
  }, []);

  const handleSave = () => {
    localStorage.setItem(
      "app_settings",
      JSON.stringify({ theme, textSize, language })
    );
    alert("Ustawienia aplikacji zapisane");
  };

  return (
    <Box sx={{ p: 4 }}>
      <Paper elevation={1} sx={{ p: 3, maxWidth: 720 }}>
        <Typography variant="h6" gutterBottom>
          Ustawienia aplikacji
        </Typography>

        <Box sx={{ display: "grid", gap: 2 }}>
          <FormControl>
            <InputLabel id="theme-label">Motyw</InputLabel>
            <Select
              labelId="theme-label"
              label="Motyw"
              value={theme}
              onChange={(e) => setTheme(e.target.value as string)}
            >
              <MenuItem value="light">Jasny</MenuItem>
              <MenuItem value="dark">Ciemny</MenuItem>
            </Select>
          </FormControl>

          <FormControl>
            <InputLabel id="textsize-label">Rozmiar tekstu</InputLabel>
            <Select
              labelId="textsize-label"
              label="Rozmiar tekstu"
              value={textSize}
              onChange={(e) => setTextSize(e.target.value as string)}
            >
              <MenuItem value="sm">Mały</MenuItem>
              <MenuItem value="md">Średni</MenuItem>
              <MenuItem value="lg">Duży</MenuItem>
            </Select>
          </FormControl>

          <FormControl>
            <InputLabel id="lang-label">Język</InputLabel>
            <Select
              labelId="lang-label"
              label="Język"
              value={language}
              onChange={(e) => setLanguage(e.target.value as string)}
            >
              <MenuItem value="pl">Polski</MenuItem>
              <MenuItem value="en">English</MenuItem>
            </Select>
          </FormControl>

          <Box sx={{ display: "flex", gap: 1, justifyContent: "flex-end" }}>
            <Button variant="contained" onClick={handleSave}>
              Zapisz
            </Button>
          </Box>
        </Box>
      </Paper>
    </Box>
  );
}
