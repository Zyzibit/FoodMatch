import { Box, Typography, Fade } from "@mui/material";
import { colors } from "../../theme";

interface FieldErrorProps {
  message?: string;
}

export function FieldError({ message }: FieldErrorProps) {
  return (
    <Fade in={!!message} timeout={150} mountOnEnter unmountOnExit>
      <Box
        sx={{
          position: "absolute",
          left: "8px",
          bottom: "-16px",
          pointerEvents: "none",
        }}
      >
        <Typography
          variant="caption"
          sx={{
            color: colors.feedback.error,
            fontSize: "0.75rem",
          }}
        >
          {message}
        </Typography>
      </Box>
    </Fade>
  );
}
