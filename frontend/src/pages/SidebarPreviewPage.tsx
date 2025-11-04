import { useCallback, useMemo, useState } from "react";
import { Box, Paper, Typography } from "@mui/material";
import SidebarPanel from "../components/panels/SidebarPanel";
import TopPanel from "../components/panels/TopPanel";
import SettingsPage from "./SettingsPage";
import UserProfilePage from "./UserProfilePage";
import Footer from "../components/panels/Footer";

export default function SidebarPreviewPage() {
  // aktywna STRONA (lewy panel) i aktywna ZAKŁADKA (górny panel)
  const [activePage, setActivePage] = useState<string>("przepisy");
  const [activeTab, setActiveTab] = useState<string>("");

  // klik w lewym panelu (wybór strony)
  const handleSidebarClick = useCallback((key: string) => {
    setActivePage(key);
  }, []);

  // klik w górnym panelu (wybór zakładki)
  const handleTopChange = useCallback((key: string) => {
    setActiveTab(key);
  }, []);

  // (opcjonalnie) debug/info w środku
  const infoText = useMemo(
    () => `Strona: ${activePage || "—"} | Zakładka: ${activeTab || "—"}`,
    [activePage, activeTab]
  );

  return (
    <Box
      sx={{
        display: "flex",
        minHeight: "100vh",
        bgcolor: (t) => t.palette.grey[100],
      }}
    >
      {/* Lewy navbar – wybór strony */}
      <SidebarPanel
        activeKey={activePage}
        onItemClick={handleSidebarClick}
        onLogoutClick={() => {}}
        userName="Jan Kowalski"
      />

      {/* Prawa część: górny pasek + kontent */}
      <Box sx={{ flexGrow: 1, display: "flex", flexDirection: "column" }}>
        {/* Górny navbar – nie pokazujemy go na stronie ustawień; dla strony 'user' pokażemy specjalne zakładki */}
        {activePage !== "ustawienia" && (
          <TopPanel
            activePage={activePage}
            activeKey={activeTab}
            onChange={handleTopChange}
            sticky
          />
        )}

        {/* Kontent – render zależny od wybranej strony */}
        <Box
          component="main"
          sx={{
            flexGrow: 1,
            p: 4,
            display: "flex",
            alignItems: "flex-start",
            justifyContent: "flex-start",
          }}
        >
          {activePage === "ustawienia" ? (
            // Brak topbara - pokaż sam napis "Ustawienia" bez tła
            <Box sx={{ width: "100%" }}>
              <Typography variant="h4" sx={{ fontWeight: 800, mb: 3 }}>
                Ustawienia
              </Typography>
              {/* pod spodem zostawiamy obecny komponent ustawień */}
              <SettingsPage />
            </Box>
          ) : activePage === "user" ? (
            <Paper elevation={1} sx={{ p: 3, minWidth: 320, width: "100%" }}>
              {/* Render sekcji user zgodnie z activeTab */}
              {activeTab === "pomiary" && <UserProfilePage />}
              {activeTab === "zapotrzebowanie" && (
                <Typography variant="h6">
                  Zapotrzebowanie (placeholder)
                </Typography>
              )}
              {activeTab === "alergeny" && (
                <Typography variant="h6">Alergeny (placeholder)</Typography>
              )}
            </Paper>
          ) : (
            <Paper elevation={1} sx={{ p: 3, minWidth: 320 }}>
              <Typography variant="h6" gutterBottom>
                Kontent strony (placeholder)
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {infoText}
              </Typography>
            </Paper>
          )}
        </Box>

        {/* Footer */}
        <Footer />
      </Box>
    </Box>
  );
}
