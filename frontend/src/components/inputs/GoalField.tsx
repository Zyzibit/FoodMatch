import { Box, MenuItem, TextField, Typography } from "@mui/material";

type Goal = "lose" | "maintain" | "gain";

interface GoalFieldProps {
  value: Goal | "";
  onChange: (value: Goal) => void;
  disabled?: boolean;
  showDetails?: boolean;
}

const GOAL_OPTIONS = [
  {
    value: "lose" as const,
    label: "Zrzucić wagę",
    description: "Redukcja masy ciała",
  },
  {
    value: "maintain" as const,
    label: "Utrzymać obecną wagę",
    description: "Bez zmian wagi",
  },
  {
    value: "gain" as const,
    label: "Przybrać na wadze",
    description: "Budowa masy / mięśni",
  },
];

export function GoalField({
  value,
  onChange,
  disabled = false,
  showDetails = false,
}: GoalFieldProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value as Goal);
  };

  return (
    <TextField
      select
      label="Twój cel żywieniowy"
      value={value}
      onChange={handleChange}
      required
      fullWidth
      disabled={disabled}
      helperText="Co chcesz osiągnąć?"
    >
      {GOAL_OPTIONS.map(({ value, label, description }) => (
        <MenuItem key={value} value={value}>
          {showDetails ? (
            <Box>
              <Typography variant="body2" fontWeight={600}>
                {label}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {description}
              </Typography>
            </Box>
          ) : (
            label
          )}
        </MenuItem>
      ))}
    </TextField>
  );
}

export { GOAL_OPTIONS };
export type { Goal };
