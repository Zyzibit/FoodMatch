import { TextField } from "@mui/material";

interface AgeFieldProps {
  value: number | "";
  onChange: (value: number | "") => void;
  disabled?: boolean;
  error?: boolean;
  helperText?: string;
  minAge?: number;
  maxAge?: number;
}

export function AgeField({
  value,
  onChange,
  disabled = false,
  error = false,
  helperText,
  minAge = 7,
  maxAge = 100,
}: AgeFieldProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    onChange(val === "" ? "" : Number(val));
  };

  return (
    <TextField
      label="Wiek"
      type="number"
      value={value}
      onChange={handleChange}
      required
      fullWidth
      disabled={disabled}
      error={error}
      inputProps={{ min: minAge, max: maxAge }}
      helperText={helperText || `Podaj swój wiek (${minAge}-${maxAge} lat)`}
    />
  );
}
