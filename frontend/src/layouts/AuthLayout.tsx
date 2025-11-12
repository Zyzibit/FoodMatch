import { Box, Container, Typography } from "@mui/material";
import type { ReactNode } from "react";
import dietLogo from "../assets/diet-logo.png";

type AuthLayoutProps = {
  title?: string;
  children: ReactNode;
};
export default function AuthLayout({ children }: AuthLayoutProps) {
  return (
    <Box
      sx={{
        minHeight: "100vh",
        bgcolor: "background.default",
      }}
    >
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: { xs: "1fr", md: "1fr 480px" },
          minHeight: "100vh",
          width: "100%",
          pr: { md: 16 },
        }}
      >
        <Box
          sx={{
            display: "grid",
            placeItems: "center",
            width: "100%",
            height: "100%",
          }}
        >
          <Box
            component="img"
            src={dietLogo}
            alt="Diet logo"
            sx={{
              width: { xs: 280, sm: 380, md: 520 },
              maxWidth: "95%",
              height: "auto",
              borderRadius: 2,
              boxShadow: 2,
              objectFit: "contain",
            }}
          />
        </Box>

        <Box
          sx={{
            bgcolor: "secondary.main",
            display: "flex",
            flexDirection: "column",
            alignItems: "center",
            py: { xs: 6, md: 8 },
            px: { xs: 3, md: 4 },
            height: "100%",
          }}
        >
          <Container maxWidth={false} sx={{ width: "100%", maxWidth: 420 }}>
            <Typography
              variant="h4"
              align="center"
              sx={{
                color: "common.white",
                fontWeight: 800,
                letterSpacing: 2,
                mb: 2.5,
                fontFamily: "serif",
                lineHeight: 1.1,
              }}
            >
              {"DIET\nZYNZI".split("\n").map((line, i) => (
                <Box
                  component="span"
                  key={i}
                  sx={{ display: "block", textAlign: "center" }}
                >
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
