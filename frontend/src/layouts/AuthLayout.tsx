import { Box, Container, Typography } from "@mui/material";
import type { ReactNode } from "react";

type AuthLayoutProps = {
  title: string;              // np. "DIET ZYNZI"
  children: ReactNode;        // tu wchodzi formularz (login / register)
  panelBg?: string;           // kolor prawego panelu (default: zielony)
  maxFormWidth?: number;      // szerokość formularza
};

export default function AuthLayout({
  title,
  children,
  panelBg = "#2C8C7C",        // zielony jak na screenie
  maxFormWidth = 420,
}: AuthLayoutProps) {
  return (
    <Box sx={{ minHeight: "100vh", bgcolor: "background.default" }}>
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: { xs: "1fr", md: "1fr 420px" }, // 2 kolumny na >= md
          minHeight: "100vh",
        }}
      >
        {/* Lewa kolumna: placeholder zamiast logo */}
        <Box
          sx={{
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
            px: { xs: 2, md: 6 },
            py: { xs: 6, md: 0 },
          }}
        >
          {/* Kwadratowy placeholder */}
          <Box
            sx={{
              width: { xs: "70%", sm: 380, md: 520 },
              maxWidth: "90vw",
              aspectRatio: "1 / 1",
              bgcolor: "#ECECEC",
              borderRadius: 3,
              boxShadow: 1,
            }}
          />
        </Box>

        {/* Prawa kolumna: panel z formularzem */}
        <Box
          sx={{
            bgcolor: panelBg,
            color: "common.white",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            py: { xs: 6, md: 8 },
            px: 3,
          }}
        >
          <Container maxWidth={false} sx={{ width: "100%", maxWidth: maxFormWidth }}>
            <Typography
              variant="h4"
              align="center"
              sx={{ fontWeight: 800, letterSpacing: 2, mb: 3, fontFamily: "serif" }}
            >
              {title.split(" ").map((line, i) => (
                <Box component="span" key={i} sx={{ display: "block", lineHeight: 1.1 }}>
                  {line}
                </Box>
              ))}
            </Typography>

            {children}
          </Container>
        </Box>
      </Box>
    </Box>
  );
}
