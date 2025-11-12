import { Stack, Box, Typography, IconButton } from "@mui/material";
import {
  ChevronLeft,
  ChevronRight,
  CalendarMonth,
} from "@mui/icons-material";

type PlanDayHeaderProps = {
  consumedCalories: number;
  dateLabel: string;
  onPrev?: () => void;
  onNext?: () => void;
  onPickDate?: () => void;
};

export default function PlanDayHeader({
  consumedCalories,
  dateLabel,
  onPrev,
  onNext,
  onPickDate,
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
      <Stack direction="row" spacing={1}>
        <IconButton size="small" aria-label="Poprzedni dzień" onClick={onPrev}>
          <ChevronLeft />
        </IconButton>
        <IconButton
          size="small"
          aria-label="Wybierz dzień"
          onClick={onPickDate}
        >
          <CalendarMonth />
        </IconButton>
        <IconButton size="small" aria-label="Następny dzień" onClick={onNext}>
          <ChevronRight />
        </IconButton>
      </Stack>
    </Stack>
  );
}
