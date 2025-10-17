import type { SxProps, Theme } from "@mui/material/styles";

export const baseInputStyle: SxProps<Theme> = {
  position: "relative",
  "& .MuiInputBase-root": {
    backgroundColor: "#f0f0f0",
    borderRadius: "8px",
    color: "#333",
    paddingBottom: "4px",
  },
  "& .MuiOutlinedInput-notchedOutline": {
    border: "none",
  },
  "&:hover .MuiOutlinedInput-notchedOutline": {
    border: "none",
  },
  "& .Mui-focused .MuiOutlinedInput-notchedOutline": {
    border: "none",
  },
  "& .MuiInputBase-input::placeholder": {
    color: "#777",
    opacity: 1,
  },
  "& .MuiInputLabel-root": {
    display: "none",
  },
};
