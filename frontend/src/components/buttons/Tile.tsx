import { Button, Box, Typography } from "@mui/material";
import type { ReactNode, MouseEventHandler } from "react";
import { alpha } from "@mui/material/styles";

export type TileSize = "sm" | "md" | "lg";

export interface TileProps {
  title: string;
  icon?: ReactNode;
  active?: boolean;
  disabled?: boolean;
  size?: TileSize;
  square?: boolean; // if true, remove border radius (useful for date strip)
  bordered?: boolean; // optional border (handled usually by parent)
  fullHeight?: boolean; // when true, tile stretches to parent's height
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
  square = false,
  bordered = false,
  fullHeight = false,
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
        const isLight = t.palette.mode === "light";
        const lightNeutral = "#c0c4c9";
        const lightNeutralHover = "#b5bac0";
        const neutralBg = isLight
          ? lightNeutral
          : alpha(t.palette.common.white, 0.08);
        const neutralHover = isLight
          ? lightNeutralHover
          : alpha(t.palette.common.white, 0.18);
        const activeBg = t.palette.secondary.main;
        const activeColor = t.palette.getContrastText(activeBg);

        return {
          justifyContent: "center",
          borderRadius: square ? 0 : 9999,
          py: fullHeight ? 0 : paddings[size],
          px: 2,
          textTransform: "none",
          boxShadow: "none",
          height: fullHeight ? "100%" : heights[size],
          backgroundColor: active ? activeBg : neutralBg,
          color: active ? activeColor : t.palette.text.primary,
          overflow: "hidden",
          border: bordered
            ? `1px solid ${t.palette.divider}`
            : `1px solid ${
                active
                  ? alpha(activeBg, 0.6)
                  : alpha(t.palette.common.black, isLight ? 0.08 : 0.2)
              }`,
          transition:
            "background-color 0.2s ease, color 0.2s ease, box-shadow 0.2s",

          "&:hover": {
            backgroundColor: active ? activeBg : neutralHover,
            color: active ? activeColor : t.palette.text.primary,
          },
          "&.Mui-disabled": {
            backgroundColor:
              t.palette.mode === "dark"
                ? alpha(t.palette.common.white, 0.04)
                : t.palette.action.disabledBackground,
            color: t.palette.text.disabled,
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
