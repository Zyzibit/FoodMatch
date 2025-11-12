import { alpha } from "@mui/material/styles";
import type { SxProps, Theme } from "@mui/material/styles";

export const baseInputStyle: SxProps<Theme> = {
  position: "relative",
  "& .MuiInputBase-root": {
    backgroundColor: (t) => alpha(t.palette.background.paper, t.palette.mode === "light" ? 0.98 : 0.08),
    borderRadius: "10px",
    color: (t) => t.palette.text.primary,
    paddingBottom: "4px",
    transition: "box-shadow 150ms ease, border-color 150ms ease",
    boxShadow: (t) =>
      t.palette.mode === "light"
        ? `0 1px 2px ${alpha(t.palette.common.black, 0.08)}`
        : "none",
  },
  "& .MuiOutlinedInput-notchedOutline": {
    borderColor: (t) => alpha(t.palette.text.primary, 0.2),
  },
  "&:hover .MuiOutlinedInput-notchedOutline": {
    borderColor: (t) => alpha(t.palette.text.primary, 0.35),
  },
  "& .Mui-focused .MuiOutlinedInput-notchedOutline": {
    borderColor: (t) => t.palette.primary.main,
    borderWidth: "1.5px",
  },
  "& .MuiInputBase-input::placeholder": {
    color: (t) => alpha(t.palette.text.secondary, 0.8),
    opacity: 1,
  },
  "& .MuiInputAdornment-root, & .MuiIconButton-root": {
    color: (t) => t.palette.text.secondary,
  },
  "& .MuiInputLabel-root": { display: "none" },
};
