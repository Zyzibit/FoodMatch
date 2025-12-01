import { useCallback, useMemo, useState } from "react";
import { Box } from "@mui/material";
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

const SUPPORTED_PAGES = new Set([
  "plan",
  "lista",
  "przepisy",
  "ustawienia",
  "user",
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

  const handleSidebarClick = useCallback(
    (key: string) => {
      if (!SUPPORTED_PAGES.has(key)) return;
      navigate(`/app/${key}`);
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
      <Box sx={{ flexShrink: 0, height: "100%" }}>
        <SidebarPanel
          activeKey={activePage}
          onItemClick={handleSidebarClick}
          onLogoutClick={handleLogout}
          userName={user?.username || "User"}
        />
      </Box>

      <Box
        sx={{
          flexGrow: 1,
          display: "flex",
          flexDirection: "column",
          height: "100%",
        }}
      >
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
            p: 4,
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
