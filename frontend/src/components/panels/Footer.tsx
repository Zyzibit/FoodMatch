import { Box } from "@mui/material";

export default function Footer() {
  const HEIGHT = 56;

  return (
    <Box
      sx={(t) => ({
        width: "100%",
        height: HEIGHT,
        bgcolor: t.palette.background.paper,
        borderTop: `1px solid ${t.palette.divider}`,
        boxShadow:
          t.palette.mode === "dark"
            ? "0 -4px 20px rgba(0,0,0,0.35)"
            : "0 -4px 18px rgba(15,40,77,0.08)",
      })}
    />
  );
}
