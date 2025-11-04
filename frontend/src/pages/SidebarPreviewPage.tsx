import { useCallback, useMemo, useState } from "react";
import { Box, Paper, Typography } from "@mui/material";
import SidebarPanel from "../components/panels/SidebarPanel";
import TopPanel from "../components/panels/TopPanel";

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
        {/* Górny navbar – dynamicznie zdefiniowane zakładki */}
        <TopPanel
          activePage={activePage}
          activeKey={activeTab}
          onChange={handleTopChange}
          sticky
        />

        {/* Kontent – tu wyrenderujesz podstrony; na razie placeholder */}
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
          <Paper elevation={1} sx={{ p: 3, minWidth: 320 }}>
            <Typography variant="h6" gutterBottom>
              Kontent strony (placeholder)
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {infoText}
            </Typography>
          </Paper>
        </Box>
      </Box>
    </Box>
  );
}
