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
import { useAppTheme } from "../contexts/AppThemeContext";
import type { ThemeName } from "../theme";
import { useTextSize } from "../contexts/TextSizeContext";
import type { TextSize } from "../contexts/TextSizeContext";

export default function SettingsPage() {
  const { themeName, setThemeName } = useAppTheme();
  const { textSize, setTextSize } = useTextSize();
  const [language, setLanguage] = useState<string>("pl");

  useEffect(() => {
    try {
      const raw = localStorage.getItem("app_settings");
      if (!raw) return;
      const parsed = JSON.parse(raw);
      if (parsed.theme) {
        setThemeName(parsed.theme as ThemeName);
      }
      if (parsed.textSize) setTextSize(parsed.textSize as TextSize);
      if (parsed.language) setLanguage(parsed.language);
    } catch (e) {
      // ignore
    }
  }, [setThemeName, setTextSize]);

  const handleSave = () => {
    localStorage.setItem(
      "app_settings",
      JSON.stringify({ theme: themeName, textSize, language })
    );
    alert("Ustawienia aplikacji zapisane");
  };

  return (
    <Box
      sx={{
        width: "100%",
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
      }}
    >
      <Typography variant="h4" sx={{ fontWeight: 800, mb: 3, width: "100%", maxWidth: 720 }}>
        Ustawienia
      </Typography>

      <Paper elevation={1} sx={{ p: 3, width: "100%", maxWidth: 720 }}>
        <Typography variant="h6" gutterBottom>
          Ustawienia aplikacji
        </Typography>

        <Box sx={{ display: "grid", gap: 2 }}>
          <FormControl>
            <InputLabel id="theme-label">Motyw</InputLabel>
            <Select
              labelId="theme-label"
              label="Motyw"
              value={themeName}
              onChange={(e) => setThemeName(e.target.value as ThemeName)}
            >
              <MenuItem value="light">Jasny</MenuItem>
              <MenuItem value="dark">Ciemny</MenuItem>
              <MenuItem value="halloween">Halloween</MenuItem>
              <MenuItem value="winter">Zima</MenuItem>
              <MenuItem value="spring">Wiosna</MenuItem>
              <MenuItem value="summer">Lato</MenuItem>
              <MenuItem value="forest">Leśny</MenuItem>
              <MenuItem value="sunset">Zachód słońca</MenuItem>
              <MenuItem value="ocean">Ocean</MenuItem>
            </Select>
          </FormControl>

          <FormControl>
            <InputLabel id="textsize-label">Rozmiar tekstu</InputLabel>
            <Select
              labelId="textsize-label"
              label="Rozmiar tekstu"
              value={textSize}
              onChange={(e) => setTextSize(e.target.value as TextSize)}
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
