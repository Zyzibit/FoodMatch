import { Box, MenuItem, TextField, Typography } from "@mui/material";
import {
  FITNESS_GOAL_OPTIONS,
  type FitnessGoal,
  type FitnessGoalOption,
} from "../../constants/fitnessGoals";

interface GoalFieldProps {
  value: FitnessGoal | "";
  onChange: (value: FitnessGoal) => void;
  disabled?: boolean;
  showDetails?: boolean;
}

const GOAL_OPTIONS: FitnessGoalOption[] = FITNESS_GOAL_OPTIONS;

export function GoalField({
  value,
  onChange,
  disabled = false,
  showDetails = false,
}: GoalFieldProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value as FitnessGoal);
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
      {GOAL_OPTIONS.map(({ value, label, description, adjustmentNote }) => (
        <MenuItem key={value} value={value}>
          {showDetails ? (
            <Box>
              <Typography variant="body2" fontWeight={600}>
                {label}
              </Typography>
              <Typography variant="caption" color="text.secondary">
                {description} · {adjustmentNote}
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
export type { FitnessGoal };
