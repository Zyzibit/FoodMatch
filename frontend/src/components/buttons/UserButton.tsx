import { Button, Avatar, Typography } from "@mui/material";
import { alpha } from "@mui/material/styles";
import type { MouseEventHandler } from "react";

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
      sx={(t) => ({
        display: "flex",
        alignItems: "center",
        justifyContent: "flex-start",
        gap: 1,
        px: 1.5,
        py: 0.5,
        borderRadius: 20,
        backgroundColor:
          t.palette.mode === "dark"
            ? alpha(t.palette.common.white, 0.12)
            : alpha(t.palette.text.primary, 0.05),
        color: t.palette.text.primary,
        textTransform: "none",
        fontWeight: 600,
        "&:hover": {
          backgroundColor:
            t.palette.mode === "dark"
              ? alpha(t.palette.common.white, 0.2)
              : alpha(t.palette.text.primary, 0.1),
        },
      })}
    >
      <Avatar
        src={avatarUrl}
        alt={name}
        sx={(t) => ({
          width: 28,
          height: 28,
          border: `2px solid ${
            t.palette.mode === "dark"
              ? alpha(t.palette.common.white, 0.4)
              : alpha(t.palette.common.black, 0.1)
          }`,
        })}
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
