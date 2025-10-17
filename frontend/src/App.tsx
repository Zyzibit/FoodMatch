import React from "react";
import { CssBaseline, ThemeProvider } from "@mui/material";
import { theme } from "./theme";
import { RegisterForm } from "./components/forms/RegisterForm/RegisterForm";

export default function App() {
  return (
    <ThemeProvider theme={theme}>
      <CssBaseline />
      <RegisterForm />
    </ThemeProvider>
  );
}
