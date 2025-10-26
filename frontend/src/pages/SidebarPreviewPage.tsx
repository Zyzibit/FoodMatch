import { useCallback, useState } from "react";
import { Box, Paper, Typography } from "@mui/material";
import SidebarPanel from "../components/panels/SidebarPanel";
import type { SidebarPanelItem } from "../components/panels/SidebarPanel";

export default function SidebarPreviewPage() {
  const [message, setMessage] = useState("Wybierz opcje z panelu bocznego.");
  const [activeItem, setActiveItem] = useState<SidebarPanelItem | undefined>();

  const handleNavigation = useCallback(
    (item: SidebarPanelItem, label: string) => () => {
      setActiveItem(item);
      setMessage(`Kliknieto: ${label}`);
    },
    []
  );

  const handleLogout = useCallback(() => {
    setMessage("Kliknieto: Wyloguj");
  }, []);

  return (
    <Box
      sx={{
        display: "flex",
        minHeight: "100vh",
        bgcolor: (theme) => theme.palette.grey[100],
      }}
    >
      <SidebarPanel
        onPlanClick={handleNavigation("plan", "Plan dietetyczny")}
        onListClick={handleNavigation("list", "Lista zakupow")}
        onRecipesClick={handleNavigation("recipes", "Przepisy")}
        onSettingsClick={handleNavigation("settings", "Ustawienia")}
        onLogoutClick={handleLogout}
        userName="Jan Kowalski"
        activeItem={activeItem}
      />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          p: 6,
        }}
      >
        <Paper elevation={2} sx={{ maxWidth: 480, p: 4 }}>
          <Typography variant="h4" gutterBottom>
            Podglad panelu bocznego
          </Typography>
          <Typography variant="body1">{message}</Typography>
        </Paper>
      </Box>
    </Box>
  );
}
