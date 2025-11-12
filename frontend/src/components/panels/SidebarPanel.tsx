import { Box, Stack, Typography, Button } from "@mui/material";
import Tile from "../buttons/Tile";
import UserButton from "../buttons/UserButton";
import dietLogo from "../../assets/diet-logo.png";

export default function SidebarPanel({
  activeKey,
  onItemClick,
  onLogoutClick,
  userName = "User",
  userAvatar,
}: {
  activeKey?: string;
  onItemClick?: (key: string) => void;
  onLogoutClick?: () => void;
  userName?: string;
  userAvatar?: string;
}) {
  const sidebarItems = [
    { key: "plan", title: "Plan" },
    { key: "lista", title: "Lista" },
    { key: "przepisy", title: "Przepisy" },
    { key: "ustawienia", title: "Ustawienia" },
  ];
  return (
    <Box
      sx={(t) => ({
        width: 420,
        height: "100vh",
        bgcolor: t.palette.background.paper,
        color: t.palette.text.primary,
        display: "flex",
        flexDirection: "column",
        justifyContent: "space-between",
        alignItems: "center",
        py: 3,
        borderRight: `1px solid ${t.palette.divider}`,
        backgroundImage:
          t.palette.mode === "dark"
            ? "linear-gradient(180deg, rgba(77,182,172,0.1), transparent 70%)"
            : "linear-gradient(180deg, rgba(44,140,124,0.08), transparent 65%)",
      })}
    >
      {/* logo + tytuł */}
      <Stack direction="row" alignItems="center" spacing={1.5}>
        <Box
          component="img"
          src={dietLogo}
          alt="Logo"
          sx={{ width: 112, height: 112 }}
        />
        <Typography
          variant="h4"
          sx={{
            color: "secondary.main",
            lineHeight: 1.05,
            textAlign: "center",
          }}
        >
          <Box component="div" sx={{ fontWeight: 900 }}>
            DIET
          </Box>
          <Box component="div" sx={{ fontWeight: 900 }}>
            ZYNZI
          </Box>
        </Typography>
      </Stack>

      {/* sekcja kafelków */}
      <Stack
        spacing={1.2}
        alignItems="center"
        sx={{ width: "100%", flexGrow: 1, justifyContent: "flex-start", pt: 4 }}
      >
        <Box sx={{ width: "80%" }}>
          <Typography
            variant="h5"
            sx={{ fontWeight: 900, mb: 1.5, textAlign: "left" }}
          >
            Zdrowie
          </Typography>
        </Box>

        <Stack spacing={2} sx={{ width: "80%" }}>
          {sidebarItems.map(({ key, title }) => (
            <Tile
              key={key}
              title={title}
              active={key === activeKey}
              onClick={() => onItemClick?.(key)}
            />
          ))}
        </Stack>
      </Stack>

      {/* dolna sekcja */}
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        sx={{ width: "80%" }}
      >
        <UserButton
          name={userName}
          avatarUrl={userAvatar}
          onClick={() => onItemClick?.("user")}
        />
        <Button
          onClick={onLogoutClick}
          disableElevation
          sx={(t) => ({
            px: 1.5,
            py: 0.5,
            borderRadius: 20,
            textTransform: "none",
            fontWeight: 600,
            backgroundColor:
              t.palette.mode === "dark"
                ? "rgba(255,255,255,0.1)"
                : t.palette.background.default,
            color: t.palette.text.primary,
            boxShadow: "none",
            "&:hover": {
              backgroundColor:
                t.palette.mode === "dark"
                  ? "rgba(255,255,255,0.2)"
                  : t.palette.action.hover,
            },
          })}
        >
          Wyloguj
        </Button>
      </Stack>
    </Box>
  );
}
