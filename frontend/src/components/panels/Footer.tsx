import { Box } from "@mui/material";
import { colors } from "../../theme";

export default function Footer() {
  const HEIGHT = 56;

  return (
    <Box
      sx={(t) => ({
        width: "100%",
        height: HEIGHT,
        bgcolor: colors.elements.tileNeutral,
        borderTop: `1px solid ${t.palette.grey[300]}`,
      })}
    />
  );
}
