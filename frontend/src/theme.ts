import { createTheme } from "@mui/material/styles";
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
    background: "#e2e2e2ff", // Ogolne tlo aplikacji (theme.palette.background.default)
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

export const theme = createTheme(
  {
    palette: {
      mode: "light", // Cała aplikacja pracuje w trybie jasnym

      primary: {
        main: colors.brand.primary.main, // Domyślny kolor CTA (przyciski logowania/rejestracji)
        light: colors.brand.primary.light, // Jaśniejsza odmiana CTA, np. dla stanów focus
        dark: colors.brand.primary.dark, // Ciemniejsza odmiana CTA, np. aktywne kafelki
      },
      secondary: {
        main: colors.brand.secondary.main, // Tło panelu logowania/rejestracji w AuthLayout
        light: colors.brand.secondary.light, // Jasna odmiana panelu, np. przyszłe hovery
        dark: colors.brand.secondary.dark, // Ciemna odmiana panelu, np. aktywne Tile
      },
      background: {
        default: colors.layout.background, // Ogólne tło aplikacji (body/strony)
        paper: colors.layout.paper, // Tło kart i kontenerów MUI
      },
      text: {
        primary: colors.text.primary, // Główne treści tekstowe (SidebarPanel, formularze)
        secondary: colors.text.secondary, // Opisy i pomocnicze etykiety na jasnym tle
      },
      grey: {
        100: colors.grey[100], // Jasne obramowania/hovery na komponentach
        300: colors.grey[300], // Neutralne linie i podziały
        500: colors.grey[500], // Tekst nieaktywny/disabled
      },
      error: {
        main: colors.feedback.error, // Powiązany z FieldError (wyświetla czerwony tekst)
      },
      common: {
        white: colors.text.onAccentSolid, // Współdzielona biel dla nagłówków na tle secondary
      },
    },

    shape: {
      borderRadius: 12, // Domyslne zaokraglenie kart/formularzy w calej aplikacji
    },

    typography: {
      fontFamily: ["Inter", "Roboto", "Helvetica", "Arial", "sans-serif"].join(","), // Spójna czcionka w UI
    },

    components: {
      MuiButton: {
        styleOverrides: {
          containedPrimary: {
            borderRadius: "10px", // Dopasowanie do przyciskow w LoginForm/RegisterForm
            textTransform: "none", // Zachowujemy oryginalny zapis tekstu przycisku
            fontWeight: 700, // Wyzsza waga dla CTA
            boxShadow: `0 6px 16px ${colors.brand.primary.shadow}`, // Cien takich przyciskow
            ":hover": {
              backgroundColor: colors.brand.primary.hover, // Hover CTA
              boxShadow: `0 8px 20px ${colors.brand.primary.shadowHover}`, // Wiecej cienia na hover
            },
          },
        },
      },
    },
  },
  plPL
);
