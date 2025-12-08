import { Stack, Box, Typography, Button } from "@mui/material";
import { PictureAsPdf } from "@mui/icons-material";

type PlanDayHeaderProps = {
  consumedCalories: number;
  dateLabel: string;
  onPrev?: () => void;
  onNext?: () => void;
  onPickDate?: () => void;
  onExportPdf?: () => void;
};

export default function PlanDayHeader({
  consumedCalories,
  dateLabel,
  onPrev,
  onNext,
  onPickDate,
  onExportPdf,
}: PlanDayHeaderProps) {
  return (
    <Stack
      direction={{ xs: "column", sm: "row" }}
      alignItems={{ xs: "flex-start", sm: "center" }}
      justifyContent="space-between"
      spacing={2}
    >
      <Box>
        <Typography variant="h3" fontWeight={800} sx={{ lineHeight: 1 }}>
          {consumedCalories} KCAL
        </Typography>
        <Typography variant="body2" color="text.secondary">
          {dateLabel}
        </Typography>
      </Box>
      <Button
        variant="contained"
        startIcon={<PictureAsPdf />}
        onClick={onExportPdf}
      >
        Eksportuj do PDF
      </Button>
    </Stack>
  );
}
