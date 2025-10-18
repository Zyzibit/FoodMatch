import { createTheme } from "@mui/material/styles";
import { plPL } from "@mui/material/locale";

export const theme = createTheme(
  {
    palette: {
      mode: "light",

      // 🎨 Główne kolory aplikacji
      primary: {
        main: "#3D3BFF",        // niebieski – przyciski, akcenty
        light: "#6260ff",
        dark: "#2c2acc",
      },
      secondary: {
        main: "#2C8C7C",        // zielony – panel logowania/rejestracji
        light: "#47a592",
        dark: "#1f6d60",
      },
      background: {
        default: "#e2e2e2ff",     // tło strony (białe pole)
        paper: "#FFFFFF",       // tło elementów (np. kart)
      },
      text: {
        primary: "#111827",     // ciemny tekst
        secondary: "#6B7280",   // szary tekst
      },
      grey: {
        100: "#F0F0F0",         // tło inputów
        300: "#D1D5DB",
        500: "#9CA3AF",         // placeholder
      },
    },

    shape: {
      borderRadius: 12,
    },

    typography: {
      fontFamily: ["Inter", "Roboto", "Helvetica", "Arial", "sans-serif"].join(","),
    },

    components: {
      // 🔘 globalny wygląd przycisków
      MuiButton: {
        styleOverrides: {
          containedPrimary: {
            borderRadius: "10px",
            textTransform: "none",
            fontWeight: 700,
            boxShadow: "0 6px 16px rgba(61,59,255,0.25)",
            ":hover": {
              backgroundColor: "#3432E6",
              boxShadow: "0 8px 20px rgba(61,59,255,0.35)",
            },
          },
        },
      },
    },
  },
  plPL
);
