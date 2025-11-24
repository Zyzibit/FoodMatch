import { TextField } from "@mui/material";

interface WeightFieldProps {
  value: number | "";
  onChange: (value: number | "") => void;
  disabled?: boolean;
  error?: boolean;
  helperText?: string;
  minWeight?: number;
  maxWeight?: number;
}

export function WeightField({
  value,
  onChange,
  disabled = false,
  error = false,
  helperText,
  minWeight = 30,
  maxWeight = 250,
}: WeightFieldProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    onChange(val === "" ? "" : Number(val));
  };

  return (
    <TextField
      label="Waga (kg)"
      type="number"
      value={value}
      onChange={handleChange}
      required
      fullWidth
      disabled={disabled}
      error={error}
      inputProps={{ min: minWeight, max: maxWeight, step: 0.1 }}
      helperText={
        helperText || `Podaj swoją wagę w kg (${minWeight}-${maxWeight} kg)`
      }
    />
  );
}
