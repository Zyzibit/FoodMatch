import {
  createTheme,
  type ThemeOptions,
  type Theme,
} from "@mui/material/styles";
import { plPL } from "@mui/material/locale";

export const colors = {
  brand: {
    primary: {
      main: "#3D3BFF", // Glowny kolor CTA; np. przyciski submit w LoginForm/RegisterForm
      light: "#6260ff", // Jasny akcent do stanow focus/hover; pasuje do primary.main
      dark: "#2c2acc", // Ciemniejsza wersja primary; uzywana przy stanach active
      hover: "#3432E6", // Hover dla primary.contained w override poniżej
      shadow: "rgba(61,59,255,0.25)", // Cień pod przyciskiem primary (Login/Register)
      shadowHover: "rgba(61,59,255,0.35)", // Cień dla primary po najechaniu
    },
    secondary: {
      main: "#2C8C7C", // Tlo panelu logowania/rejestracji w AuthLayout
      light: "#47a592", // Lzejsza odmiana secondary; do wykorzystania na hover
      dark: "#1f6d60", // Ciemniejsza secondary; np. aktywny Tile
    },
  },
  layout: {
    background: "#f4f6fb", // Ogolne tlo aplikacji (theme.palette.background.default)
    paper: "#FFFFFF", // Biale powierzchnie kart/dialogow (theme.palette.background.paper)
    sidebar: "#d1d3d7", // Kolor tla bocznego panelu (SidebarPanel)
  },
  text: {
    primary: "#111827", // Tekst na jasnych tlach (np. SidebarPanel, formularze)
    secondary: "#6B7280", // Opisy/pomocnicze teksty na jasnym tle
    onAccentSoft: "rgba(255,255,255,0.85)", // Tekst pomocniczy na tle secondary (Login/Register)
    onAccentStrong: "rgba(255,255,255,0.95)", // Linki akcji na tle secondary (Login/Register)
    onAccentSolid: "#FFFFFF", // Bialy tekst na akcentach (naglowek AuthLayout)
  },
  elements: {
    tileNeutral: "#c0c4c9", // Neutralne tlo kafelkow Tile gdy nieaktywne
    tileNeutralHover: "#b5bac0", // Hover kafelkow Tile gdy nieaktywne
    logoutButton: "#f3f3f3", // Wspolne tlo przyciskow Wyloguj i Profil w SidebarPanel
    logoutButtonHover: "#e8e8e8", // Hover dla przyciskow Wyloguj/Profil
    userAvatarBorder: "rgba(0,0,0,0.1)", // Obrys avatara w UserButton
    dividerOnAccent: "rgba(255,255,255,0.15)", // Dividery w formularzach na tle secondary
    openFoodFactsBadge: "#718096", // Kolor znacznika "produkt z bazy openfoodfacts"
  },
  feedback: {
    error: "#d32f2f", // Kolor komunikatu FieldError przy walidacji formularzy
  },
  grey: {
    100: "#F0F0F0", // Jasne obramowania/tla pomocnicze
    300: "#D1D5DB", // Neutralne obramowania (np. komponenty MUI)
    500: "#9CA3AF", // Kolor tekstu elementow disabled
  },
};

const baseThemeOptions: ThemeOptions = {
  palette: {
    mode: "light",
    primary: {
      main: colors.brand.primary.main,
      light: colors.brand.primary.light,
      dark: colors.brand.primary.dark,
    },
    secondary: {
      main: colors.brand.secondary.main,
      light: colors.brand.secondary.light,
      dark: colors.brand.secondary.dark,
    },
    background: {
      default: colors.layout.background,
      paper: colors.layout.paper,
    },
    text: {
      primary: colors.text.primary,
      secondary: colors.text.secondary,
    },
    grey: {
      100: colors.grey[100],
      300: colors.grey[300],
      500: colors.grey[500],
    },
    error: {
      main: colors.feedback.error,
    },
    common: {
      white: colors.text.onAccentSolid,
    },
  },
  shape: {
    borderRadius: 12,
  },
  typography: {
    fontFamily: ["Lora", "Georgia", "Times New Roman", "serif"].join(","),
    h1: {
      fontFamily: ["Playfair Display", "Georgia", "serif"].join(","),
      fontWeight: 900,
    },
    h2: {
      fontFamily: ["Playfair Display", "Georgia", "serif"].join(","),
      fontWeight: 900,
    },
    h3: {
      fontFamily: ["Playfair Display", "Georgia", "serif"].join(","),
      fontWeight: 900,
    },
    h4: {
      fontFamily: ["Playfair Display", "Georgia", "serif"].join(","),
      fontWeight: 800,
    },
    h5: {
      fontFamily: ["Playfair Display", "Georgia", "serif"].join(","),
      fontWeight: 800,
    },
    h6: {
      fontFamily: ["Playfair Display", "Georgia", "serif"].join(","),
      fontWeight: 700,
    },
  },
  components: {
    MuiButton: {
      styleOverrides: {
        containedPrimary: {
          borderRadius: "10px",
          textTransform: "none",
          fontWeight: 700,
          boxShadow: `0 6px 16px ${colors.brand.primary.shadow}`,
          ":hover": {
            backgroundColor: colors.brand.primary.hover,
            boxShadow: `0 8px 20px ${colors.brand.primary.shadowHover}`,
          },
        },
      },
    },
  },
};

const createAppTheme = (overrides?: ThemeOptions) =>
  createTheme(baseThemeOptions, overrides ?? {}, plPL);

export const themes: Record<string, Theme> = {
  light: createAppTheme({
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage:
              "radial-gradient(circle at top, rgba(61,59,255,0.08), transparent 60%), radial-gradient(circle at 20% 20%, rgba(44,140,124,0.15), transparent 55%)",
            backgroundAttachment: "fixed",
          },
        },
      },
    },
  }),
  dark: createAppTheme({
    palette: {
      mode: "dark",
      background: {
        default: "#0f1117",
        paper: "#1b1f2b",
      },
      text: {
        primary: "#f4f7ff",
        secondary: "#b0b8d0",
      },
      secondary: {
        main: "#4db6ac",
      },
    },
    typography: {
      fontFamily: ["Inter", "Arial", "sans-serif"].join(","),
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage:
              "radial-gradient(circle at 10% 20%, rgba(77,182,172,0.25), transparent 40%), radial-gradient(circle at 80% 0%, rgba(63,81,181,0.3), transparent 45%)",
            backgroundColor: "#0f1117",
            color: "#f4f7ff",
          },
        },
      },
    },
  }),
  halloween: createAppTheme({
    palette: {
      primary: { main: "#ff9100" },
      secondary: { main: "#4e1b49" },
      background: {
        default: "#1a0f1f",
        paper: "#2a1630",
      },
      text: {
        primary: "#ffe8c2",
        secondary: "#ddb5ff",
      },
    },
    typography: {
      fontFamily: ['"Creepster"', '"Lora"', "serif"].join(","),
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage:
              "linear-gradient(135deg, #140a22 0%, #30102b 40%, #140a22 100%)",
            color: "#ffe8c2",
          },
        },
      },
    },
  }),
  winter: createAppTheme({
    palette: {
      primary: { main: "#1e6091" },
      secondary: { main: "#a9d6e5" },
      background: {
        default: "#e5f2ff",
        paper: "#ffffff",
      },
      text: {
        primary: "#0b2545",
        secondary: "#46637f",
      },
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage:
              "linear-gradient(180deg, #f0f8ff 0%, #e5f2ff 60%, #ffffff 100%)",
          },
        },
      },
    },
  }),
  spring: createAppTheme({
    palette: {
      primary: { main: "#6ab04c" },
      secondary: { main: "#f9e79f" },
      background: {
        default: "#f4fff2",
        paper: "#ffffff",
      },
      text: {
        primary: "#2d3436",
        secondary: "#6c757d",
      },
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage:
              "linear-gradient(120deg, rgba(106,176,76,0.15), transparent 70%), linear-gradient(300deg, rgba(249,231,159,0.4), transparent 60%)",
            backgroundColor: "#f4fff2",
          },
        },
      },
    },
  }),
  summer: createAppTheme({
    palette: {
      primary: { main: "#ff6b6b" },
      secondary: { main: "#4ecdc4" },
      background: {
        default: "#fff6eb",
        paper: "#ffffff",
      },
      text: {
        primary: "#1f1f1f",
        secondary: "#525252",
      },
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage:
              "linear-gradient(135deg, rgba(255,107,107,0.1), transparent 55%), linear-gradient(225deg, rgba(78,205,196,0.15), transparent 65%)",
            backgroundColor: "#fff6eb",
          },
        },
      },
    },
  }),
  forest: createAppTheme({
    palette: {
      primary: { main: "#2f5233" },
      secondary: { main: "#a3b18a" },
      background: {
        default: "#101f14",
        paper: "#18281c",
      },
      text: {
        primary: "#f0f5f1",
        secondary: "#c9d9ce",
      },
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage:
              "linear-gradient(180deg, rgba(16,31,20,1) 0%, rgba(24,40,28,0.9) 60%, rgba(26,54,33,0.85) 100%), radial-gradient(circle at 20% 20%, rgba(163,177,138,0.25), transparent 45%)",
            color: "#f0f5f1",
          },
        },
      },
    },
  }),
  sunset: createAppTheme({
    palette: {
      primary: { main: "#ff8c42" },
      secondary: { main: "#ff5f7e" },
      background: {
        default: "#fff1e6",
        paper: "#ffffff",
      },
      text: {
        primary: "#2d1f2f",
        secondary: "#5c4b51",
      },
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage:
              "linear-gradient(120deg, rgba(255,140,66,0.25), transparent 60%), linear-gradient(300deg, rgba(255,95,126,0.25), transparent 70%)",
            backgroundColor: "#fff1e6",
          },
        },
      },
    },
  }),
  ocean: createAppTheme({
    palette: {
      primary: { main: "#0077b6" },
      secondary: { main: "#90e0ef" },
      background: {
        default: "#e0fbff",
        paper: "#ffffff",
      },
      text: {
        primary: "#023047",
        secondary: "#386280",
      },
    },
    components: {
      MuiCssBaseline: {
        styleOverrides: {
          body: {
            backgroundImage:
              "linear-gradient(180deg, rgba(0,119,182,0.12), transparent 70%), linear-gradient(45deg, rgba(144,224,239,0.35), transparent 65%)",
            backgroundColor: "#e0fbff",
          },
        },
      },
    },
  }),
};

export type ThemeName = keyof typeof themes;

export const theme = themes.light;
