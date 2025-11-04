import { Box, Stack, Typography, Button } from "@mui/material";
import Tile from "../buttons/Tile";
import UserButton from "../buttons/UserButton";
import dietLogo from "../../assets/diet-logo.png";
import { colors } from "../../theme";

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
      sx={{
        width: 420,
        height: "100vh",
        bgcolor: "#d8d8d8",
        display: "flex",
        flexDirection: "column",
        justifyContent: "space-between",
        alignItems: "center",
        py: 3,
      }}
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
            textAlign: "left",
          }}
        >
          <Box component="div" sx={{ fontWeight: 900, ml: 2 }}>
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
        <UserButton name={userName} avatarUrl={userAvatar} />
        <Button
          onClick={onLogoutClick}
          disableElevation
          sx={{
            px: 1.5,
            py: 0.5,
            borderRadius: 20,
            textTransform: "none",
            fontWeight: 600,
            backgroundColor: colors.elements.logoutButton,
            color: "text.primary",
            boxShadow: "none",
            "&:hover": { backgroundColor: colors.elements.logoutButtonHover },
          }}
        >
          Wyloguj
        </Button>
      </Stack>
    </Box>
  );
}
