import type { SxProps, Theme } from "@mui/material/styles";

export const baseInputStyle: SxProps<Theme> = {
  position: "relative",
  "& .MuiInputBase-root": {
    backgroundColor: (t) => t.palette.grey[100],
    borderRadius: "8px",
    color: (t) => t.palette.text.primary,
    paddingBottom: "4px",
  },
  "& .MuiOutlinedInput-notchedOutline": { border: "none" },
  "&:hover .MuiOutlinedInput-notchedOutline": { border: "none" },
  "& .Mui-focused .MuiOutlinedInput-notchedOutline": { border: "none" },
  "& .MuiInputBase-input::placeholder": {
    color: (t) => t.palette.grey[500],
    opacity: 1,
  },
  "& .MuiInputLabel-root": { display: "none" },
};
