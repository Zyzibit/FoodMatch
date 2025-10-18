import { Box, Typography, Fade } from "@mui/material";

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
            color: "#d32f2f",
            fontSize: "0.75rem",
          }}
        >
          {message}
        </Typography>
      </Box>
    </Fade>
  );
}
