import { Box, MenuItem, TextField, Typography } from "@mui/material";

type ActivityLevel =
  | "Sedentary"
  | "LightlyActive"
  | "ModeratelyActive"
  | "VeryActive"
  | "ExtraActive";

interface ActivityLevelFieldProps {
  value: ActivityLevel | "";
  onChange: (value: ActivityLevel) => void;
  disabled?: boolean;
  showDetails?: boolean;
}

const ACTIVITY_OPTIONS = [
  {
    value: "Sedentary" as const,
    label: "Siedzący (brak ćwiczeń)",
    shortLabel: "Siedzący",
    description: "Brak ćwiczeń, praca siedząca",
    pal: 1.2,
  },
  {
    value: "LightlyActive" as const,
    label: "Lekka aktywność (1-2 dni w tygodniu)",
    shortLabel: "Lekka aktywność",
    description: "Lekka aktywność 1–2× w tygodniu",
    pal: 1.375,
  },
  {
    value: "ModeratelyActive" as const,
    label: "Umiarkowana aktywność (3-5 dni w tygodniu)",
    shortLabel: "Umiarkowana",
    description: "Umiarkowana aktywność 3–5× w tygodniu",
    pal: 1.55,
  },
  {
    value: "VeryActive" as const,
    label: "Aktywny (6-7 dni w tygodniu)",
    shortLabel: "Aktywny",
    description: "Intensywne ćwiczenia 6–7× w tygodniu",
    pal: 1.725,
  },
  {
    value: "ExtraActive" as const,
    label: "Bardzo aktywny (sport wyczynowy)",
    shortLabel: "Bardzo aktywny",
    description: "Codzienne treningi lub praca fizyczna",
    pal: 1.9,
  },
];

export function ActivityLevelField({
  value,
  onChange,
  disabled = false,
  showDetails = false,
}: ActivityLevelFieldProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value as ActivityLevel);
  };

  return (
    <TextField
      select
      label="Poziom aktywności fizycznej"
      value={value}
      onChange={handleChange}
      required
      fullWidth
      disabled={disabled}
      helperText="Wybierz swój średni poziom aktywności"
    >
      {ACTIVITY_OPTIONS.map(
        ({ value, label, shortLabel, description, pal }) => (
          <MenuItem key={value} value={value}>
            {showDetails ? (
              <Box>
                <Typography variant="body2" fontWeight={600}>
                  {shortLabel}
                </Typography>
                <Typography variant="caption" color="text.secondary">
                  {description} · PAL {pal}
                </Typography>
              </Box>
            ) : (
              label
            )}
          </MenuItem>
        )
      )}
    </TextField>
  );
}

export { ACTIVITY_OPTIONS };
export type { ActivityLevel };
