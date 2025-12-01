import React, { useEffect, useMemo, useState } from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import { ThemeProvider, CssBaseline } from "@mui/material";
import { themes, type ThemeName } from "./theme.ts";
import AppThemeContext from "./contexts/AppThemeContext.tsx";
import TextSizeContext, { type TextSize } from "./contexts/TextSizeContext.tsx";
import { AuthProvider } from "./contexts/AuthContext.tsx";

type AppSettingsStorage = {
  theme?: ThemeName;
  textSize?: TextSize;
  language?: string;
};

const readAppSettings = (): AppSettingsStorage => {
  if (typeof window === "undefined") return {};
  const raw = window.localStorage.getItem("app_settings");
  if (!raw) return {};
  try {
    return JSON.parse(raw) as AppSettingsStorage;
  } catch {
    return {};
  }
};

const updateAppSettings = (updates: Partial<AppSettingsStorage>) => {
  if (typeof window === "undefined") return;
  const current = readAppSettings();
  const next = { ...current, ...updates };
  window.localStorage.setItem("app_settings", JSON.stringify(next));
};

const getInitialTheme = (): ThemeName => {
  if (typeof window === "undefined") return "light";
  const settings = readAppSettings();
  if (settings.theme && settings.theme in themes) {
    return settings.theme;
  }
  const stored = window.localStorage.getItem("app_theme");
  if (stored && stored in themes) return stored as ThemeName;
  return "light";
};

const getInitialTextSize = (): TextSize => {
  if (typeof window === "undefined") return "md";
  const settings = readAppSettings();
  if (settings.textSize && ["sm", "md", "lg"].includes(settings.textSize)) {
    return settings.textSize as TextSize;
  }
  const stored = window.localStorage.getItem("app_text_size");
  if (stored && ["sm", "md", "lg"].includes(stored)) {
    return stored as TextSize;
  }
  return "md";
};

const TEXT_SIZE_BASE_FONT = 16;
const TEXT_SIZE_SCALE: Record<TextSize, number> = {
  sm: 0.95,
  md: 1,
  lg: 1.1,
};

function Root() {
  const [themeName, setThemeName] = useState<ThemeName>(getInitialTheme);
  const [textSize, setTextSize] = useState<TextSize>(getInitialTextSize);

  useEffect(() => {
    if (typeof window !== "undefined") {
      window.localStorage.setItem("app_theme", themeName);
      document.body.dataset.theme = themeName;
      updateAppSettings({ theme: themeName });
    }
  }, [themeName]);

  useEffect(() => {
    if (typeof window !== "undefined") {
      window.localStorage.setItem("app_text_size", textSize);
      updateAppSettings({ textSize });
      document.body.dataset.textSize = textSize;
      const fontSizePx = TEXT_SIZE_BASE_FONT * TEXT_SIZE_SCALE[textSize];
      document.documentElement.style.fontSize = `${fontSizePx}px`;
    }
  }, [textSize]);

  const contextValue = useMemo(
    () => ({ themeName, setThemeName }),
    [themeName]
  );
  const textSizeContextValue = useMemo(
    () => ({ textSize, setTextSize }),
    [textSize]
  );

  const activeTheme = themes[themeName] ?? themes.light;

  return (
    <React.StrictMode>
      <AuthProvider>
        <AppThemeContext.Provider value={contextValue}>
          <TextSizeContext.Provider value={textSizeContextValue}>
            <ThemeProvider theme={activeTheme}>
              <CssBaseline />
              <App />
            </ThemeProvider>
          </TextSizeContext.Provider>
        </AppThemeContext.Provider>
      </AuthProvider>
    </React.StrictMode>
  );
}

ReactDOM.createRoot(document.getElementById("root")!).render(<Root />);
