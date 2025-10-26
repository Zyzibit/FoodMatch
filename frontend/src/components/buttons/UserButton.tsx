import { Button, Avatar, Typography } from "@mui/material";
import type { MouseEventHandler } from "react";
import { colors } from "../../theme";

interface UserButtonProps {
  name: string;
  avatarUrl?: string;
  onClick?: MouseEventHandler<HTMLButtonElement>;
}

export function UserButton({ name, avatarUrl, onClick }: UserButtonProps) {
  return (
    <Button
      onClick={onClick}
      disableElevation
      sx={{
        display: "flex",
        alignItems: "center",
        justifyContent: "flex-start",
        gap: 1,
        px: 1.5,
        py: 0.5,
        borderRadius: 20,
        backgroundColor: colors.elements.logoutButton,
        color: "text.primary",
        textTransform: "none",
        fontWeight: 600,
        "&:hover": {
          backgroundColor: colors.elements.logoutButtonHover,
        },
      }}
    >
      <Avatar
        src={avatarUrl}
        alt={name}
        sx={{
          width: 28,
          height: 28,
          border: `2px solid ${colors.elements.userAvatarBorder}`,
        }}
      />
      <Typography
        variant="body1"
        sx={{
          fontWeight: 600,
          color: "text.primary",
          fontSize: "0.9rem",
        }}
      >
        {name}
      </Typography>
    </Button>
  );
}

export default UserButton;
