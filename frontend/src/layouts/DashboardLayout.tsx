import { useCallback, useEffect, useMemo, useState } from "react";
import {
  AppBar,
  Box,
  Drawer,
  IconButton,
  Toolbar,
  Typography,
} from "@mui/material";
import { Menu as MenuIcon, Close as CloseIcon } from "@mui/icons-material";
import {
  Navigate,
  Outlet,
  useLocation,
  useNavigate,
  useOutletContext,
} from "react-router-dom";
import SidebarPanel from "../components/panels/SidebarPanel";
import TopPanel from "../components/panels/TopPanel";
import Footer from "../components/panels/Footer";
import { useAuth } from "../contexts/AuthContext";
import userMeasurementsService from "../services/userMeasurementsService";
import { API_BASE_URL } from "../config";

const SUPPORTED_PAGES = new Set([
  "plan",
  "lista",
  "przepisy",
  "ustawienia",
  "user",
  "admin",
]);

const resolveActivePage = (pathname: string) => {
  const [, maybePage] = pathname.split("/app/");
  if (!maybePage) return "plan";
  const key = maybePage.split("/")[0] || "plan";
  return SUPPORTED_PAGES.has(key) ? key : "plan";
};

export type DashboardOutletContext = {
  activePage: string;
  activeTab?: string;
};

export default function DashboardLayout() {
  const location = useLocation();
  const navigate = useNavigate();
  const { logout, user } = useAuth();
  const activePage = useMemo(
    () => resolveActivePage(location.pathname),
    [location.pathname]
  );

  const [pageTabs, setPageTabs] = useState<Record<string, string>>({});
  const activeTab = pageTabs[activePage];
  const [mobileNavOpen, setMobileNavOpen] = useState(false);

  // Sprawdź czy użytkownik ma wypełnione pomiary
  useEffect(() => {
    const checkMeasurements = async () => {
      try {
        const hasMeasurements = await userMeasurementsService.hasMeasurements();
        if (!hasMeasurements) {
          // Brak pomiarów - przekieruj do onboardingu
          navigate("/onboarding", { replace: true });
        }
      } catch (err) {
        console.error("Error checking measurements:", err);
        // W przypadku błędu również przekieruj do onboardingu
        navigate("/onboarding", { replace: true });
      }
    };

    checkMeasurements();
  }, [navigate]);

  const handleSidebarClick = useCallback(
    (key: string) => {
      if (!SUPPORTED_PAGES.has(key)) return;
      navigate(`/app/${key}`);
      setMobileNavOpen(false);
    },
    [navigate]
  );

  const handleLogout = useCallback(async () => {
    try {
      await logout();
      navigate("/login", { replace: true });
    } catch (error) {
      console.error("Logout failed:", error);
      // Even if logout fails, navigate to login
      navigate("/login", { replace: true });
    }
  }, [logout, navigate]);

  const handleTopChange = useCallback(
    (key: string) => {
      setPageTabs((prev) => ({ ...prev, [activePage]: key }));
    },
    [activePage]
  );

  const showTopPanel = !["ustawienia", "lista"].includes(activePage);
  const pageTitleMap: Record<string, string> = {
    plan: "Plan",
    lista: "Lista",
    przepisy: "Przepisy",
    ustawienia: "Ustawienia",
    user: "Profil",
    admin: "Panel administratora",
  };

  // Guard against unsupported direct navigation
  if (!SUPPORTED_PAGES.has(activePage)) {
    return <Navigate to="/app/plan" replace />;
  }

  return (
    <Box
      sx={{
        display: "flex",
        height: "100vh",
        overflow: "hidden",
        bgcolor: (t) => t.palette.background.default,
        color: (t) => t.palette.text.primary,
      }}
    >
      <Box sx={{ flexShrink: 0, height: "100%", display: { xs: "none", md: "block" } }}>
        <SidebarPanel
          activeKey={activePage}
          onItemClick={handleSidebarClick}
          onLogoutClick={handleLogout}
          userName={user?.username || "User"}
          userAvatar={
            user?.profilePictureUrl
              ? `${API_BASE_URL.replace("/api/v1", "")}${user.profilePictureUrl}`
              : undefined
          }
        />
      </Box>

      <Drawer
        open={mobileNavOpen}
        onClose={() => setMobileNavOpen(false)}
        ModalProps={{ keepMounted: true }}
        sx={{
          display: { xs: "block", md: "none" },
          "& .MuiDrawer-paper": {
            width: "100%",
            maxWidth: 360,
            bgcolor: (t) => t.palette.background.paper,
          },
        }}
      >
        <Box sx={{ display: "flex", justifyContent: "flex-end", p: 1 }}>
          <IconButton onClick={() => setMobileNavOpen(false)} aria-label="Zamknij menu">
            <CloseIcon />
          </IconButton>
        </Box>
        <SidebarPanel
          activeKey={activePage}
          onItemClick={handleSidebarClick}
          onLogoutClick={handleLogout}
          userName={user?.username || "User"}
          userAvatar={
            user?.profilePictureUrl
              ? `${API_BASE_URL.replace("/api/v1", "")}${user.profilePictureUrl}`
              : undefined
          }
        />
      </Drawer>

      <Box
        sx={{
          flexGrow: 1,
          display: "flex",
          flexDirection: "column",
          height: "100%",
        }}
      >
        <AppBar
          position="sticky"
          color="default"
          elevation={0}
          sx={{
            display: { xs: "flex", md: "none" },
            borderBottom: (t) => `1px solid ${t.palette.divider}`,
            bgcolor: (t) => t.palette.background.paper,
            zIndex: (t) => t.zIndex.drawer + 1,
          }}
        >
          <Toolbar sx={{ px: 2 }}>
            <IconButton
              edge="start"
              onClick={() => setMobileNavOpen(true)}
              aria-label="Otwórz menu"
              sx={{ mr: 2 }}
            >
              <MenuIcon />
            </IconButton>
            <Typography variant="subtitle1" fontWeight={800} noWrap>
              {pageTitleMap[activePage] ?? "Diet Zynzi"}
            </Typography>
          </Toolbar>
        </AppBar>

        {showTopPanel && (
          <TopPanel
            activePage={activePage}
            activeKey={activeTab}
            onChange={handleTopChange}
            sticky
          />
        )}

        <Box
          component="main"
          sx={{
            flexGrow: 1,
            p: { xs: 2, md: 4 },
            overflowY: "auto",
            display: "flex",
            alignItems: "flex-start",
            justifyContent: "flex-start",
            bgcolor: (t) => t.palette.background.default,
          }}
        >
          <Outlet context={{ activePage, activeTab }} />
        </Box>

        <Footer />
      </Box>
    </Box>
  );
}

export const useDashboardContext = () =>
  useOutletContext<DashboardOutletContext>();
