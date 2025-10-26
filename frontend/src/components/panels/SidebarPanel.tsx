import { Box, Stack, Typography, Button } from "@mui/material";
import Tile from "../buttons/Tile";
import UserButton from "../buttons/UserButton";
import dietLogo from "../../assets/diet-logo.png";
import { colors } from "../../theme";

export type SidebarPanelItem = "plan" | "list" | "recipes" | "settings";

interface SidebarPanelProps {
  onPlanClick?: () => void;
  onListClick?: () => void;
  onRecipesClick?: () => void;
  onSettingsClick?: () => void;
  onLogoutClick?: () => void;
  userName?: string;
  userAvatar?: string;
  activeItem?: SidebarPanelItem;
}

export function SidebarPanel({
  onPlanClick,
  onListClick,
  onRecipesClick,
  onSettingsClick,
  onLogoutClick,
  userName = "User",
  userAvatar = "/assets/user-avatar.png",
  activeItem,
}: SidebarPanelProps) {
  return (
    <Box
      sx={{
        width: 460,
        height: "100vh",
        bgcolor: colors.layout.sidebar,
        display: "flex",
        flexDirection: "column",
        justifyContent: "space-between",
        alignItems: "center",
        pt: 6, // było 4 → większy odstęp od góry
        pb: 6, // było 5.5
        px: 5,
      }}
    >
      {/* Logo */}
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="center"
        spacing={2.5}
        sx={{ width: "100%", mb: 4 }} // dodany większy margines poniżej logo
      >
        <Box
          component="img"
          src={dietLogo}
          alt="Logo"
          sx={{ width: 110, height: "auto", borderRadius: 2 }}
        />
        <Typography
          variant="h4"
          sx={{
            fontWeight: 800,
            color: "secondary.main",
            lineHeight: 1.05,
            letterSpacing: 1,
            textAlign: "left",
          }}
        >
          DIET
          <br />
          ZYNZI
        </Typography>
      </Stack>

      {/* Kafelki */}
      <Stack
        spacing={3} // było 1.6 → większy odstęp między sekcjami
        sx={{
          width: "100%",
          flexGrow: 1,
          justifyContent: "flex-start",
        }}
      >
        <Typography
          variant="h5"
          sx={{
            fontWeight: 800,
            mb: 2, // było 0.5 → większy odstęp pod napisem „Zdrowie”
            color: "text.primary",
          }}
        >
          Zdrowie
        </Typography>

        <Stack spacing={2.25} sx={{ width: "100%" }}>
          {/* spacing między kafelkami zwiększony z 1.75 → 2.25 */}
          <Tile
            title="Plan dietetyczny"
            onClick={onPlanClick}
            active={activeItem === "plan"}
            size="md"
          />
          <Tile
            title="Lista zakupów"
            onClick={onListClick}
            active={activeItem === "list"}
            size="md"
          />
          <Tile
            title="Przepisy"
            onClick={onRecipesClick}
            active={activeItem === "recipes"}
            size="md"
          />
          <Tile
            title="Ustawienia"
            onClick={onSettingsClick}
            active={activeItem === "settings"}
            size="md"
          />
        </Stack>
      </Stack>

      {/* Dolna sekcja: user + wylogowanie */}
      <Stack
        direction="row"
        alignItems="center"
        justifyContent="space-between"
        sx={{
          width: "100%",
          mt: 5, // było 3 → większy odstęp od kafelków
          gap: 1.5,
        }}
      >
        <UserButton name={userName} avatarUrl={userAvatar} />
        <Button
          variant="contained"
          onClick={onLogoutClick}
          sx={{
            borderRadius: 999,
            textTransform: "none",
            fontWeight: 600,
            bgcolor: colors.elements.logoutButton,
            color: "text.primary",
            boxShadow: "none",
            px: 3,
            "&:hover": { bgcolor: colors.elements.logoutButtonHover, boxShadow: "none" },
          }}
        >
          Wyloguj
        </Button>
      </Stack>
    </Box>
  );
}

export default SidebarPanel;
