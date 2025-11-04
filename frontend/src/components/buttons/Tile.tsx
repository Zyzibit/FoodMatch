import { Button, Box, Typography } from "@mui/material";
import type { ReactNode, MouseEventHandler } from "react";
import { colors } from "../../theme";

export type TileSize = "sm" | "md" | "lg";

export interface TileProps {
  title: string;
  icon?: ReactNode;
  active?: boolean;
  disabled?: boolean;
  size?: TileSize;
  onClick?: MouseEventHandler<HTMLButtonElement>;
  className?: string;
}

const paddings: Record<TileSize, number> = { sm: 0.6, md: 0.75, lg: 1.1 };
const heights: Record<TileSize, number> = { sm: 38, md: 44, lg: 56 };
const titleFontSize: Record<TileSize, string> = {
  sm: "1rem",
  md: "1rem",
  lg: "1.1rem",
};

export default function Tile({
  title,
  icon,
  active = false,
  disabled = false,
  size = "md",
  onClick,
  className,
}: TileProps) {
  return (
    <Button
      fullWidth
      disableElevation
      disabled={disabled}
      onClick={onClick}
      className={className}
      sx={(t) => {
        const neutralBg = colors.elements.tileNeutral;
        const neutralHover = colors.elements.tileNeutralHover;

        return {
          justifyContent: "center",
          borderRadius: 9999,
          py: paddings[size],
          px: 2,
          textTransform: "none",
          boxShadow: "none",
          height: heights[size],
          backgroundColor: active ? t.palette.secondary.main : neutralBg,
          color: active ? t.palette.common.white : t.palette.text.secondary,
          overflow: "hidden",
          "&:hover": {
            backgroundColor: active ? t.palette.secondary.dark : neutralHover,
          },
          "&.Mui-disabled": {
            backgroundColor: t.palette.grey[200],
            color: t.palette.grey[500],
          },
        };
      }}
    >
      <Box
        display="flex"
        alignItems="center"
        justifyContent="center"
        gap={icon ? 1.1 : 0}
        width="100%"
      >
        {icon && <Box aria-hidden>{icon}</Box>}
        <Typography
          component="div"
          sx={{
            fontWeight: 700,
            fontSize: titleFontSize[size],
            letterSpacing: 0,
            whiteSpace: "nowrap",
            overflow: "hidden",
            textOverflow: "ellipsis",
            textAlign: "center",
          }}
        >
          {title}
        </Typography>
      </Box>
    </Button>
  );
}
