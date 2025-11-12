import React, { useEffect, useMemo, useState } from "react";
import ReactDOM from "react-dom/client";
import App from "./App.tsx";
import { ThemeProvider, CssBaseline } from "@mui/material";
import { themes, type ThemeName } from "./theme.ts";
import AppThemeContext from "./contexts/AppThemeContext.tsx";

const getInitialTheme = (): ThemeName => {
  if (typeof window === "undefined") return "light";
  const stored = window.localStorage.getItem("app_theme");
  if (stored && stored in themes) return stored as ThemeName;
  return "light";
};

function Root() {
  const [themeName, setThemeName] = useState<ThemeName>(getInitialTheme);

  useEffect(() => {
    if (typeof window !== "undefined") {
      window.localStorage.setItem("app_theme", themeName);
      document.body.dataset.theme = themeName;
    }
  }, [themeName]);

  const contextValue = useMemo(
    () => ({ themeName, setThemeName }),
    [themeName]
  );

  const activeTheme = themes[themeName] ?? themes.light;

  return (
    <React.StrictMode>
      <AppThemeContext.Provider value={contextValue}>
        <ThemeProvider theme={activeTheme}>
          <CssBaseline />
          <App />
        </ThemeProvider>
      </AppThemeContext.Provider>
    </React.StrictMode>
  );
}

ReactDOM.createRoot(document.getElementById("root")!).render(<Root />);
