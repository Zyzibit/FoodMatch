import { createTheme } from "@mui/material/styles";
import { plPL } from "@mui/material/locale";

export const theme = createTheme(
  {
    cssVariables: true,
    colorSchemes: {
      light: {
        palette: {
          primary: { main: "#1976d2" },
          secondary: { main: "#9c27b0" },
        },
      },
      dark: {
        palette: {
          primary: { main: "#90caf9" },
          secondary: { main: "#ce93d8" },
        },
      },
    },
    typography: {
      fontFamily: ["Inter", "Roboto", "Helvetica", "Arial", "sans-serif"].join(
        ","
      ),
    },
  },
  plPL
);
