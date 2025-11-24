import { TextField } from "@mui/material";

interface HeightFieldProps {
  value: number | "";
  onChange: (value: number | "") => void;
  disabled?: boolean;
  error?: boolean;
  helperText?: string;
  minHeight?: number;
  maxHeight?: number;
}

export function HeightField({
  value,
  onChange,
  disabled = false,
  error = false,
  helperText,
  minHeight = 130,
  maxHeight = 300,
}: HeightFieldProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const val = e.target.value;
    onChange(val === "" ? "" : Number(val));
  };

  return (
    <TextField
      label="Wzrost (cm)"
      type="number"
      value={value}
      onChange={handleChange}
      required
      fullWidth
      disabled={disabled}
      error={error}
      inputProps={{ min: minHeight, max: maxHeight }}
      helperText={
        helperText || `Podaj swój wzrost w cm (${minHeight}-${maxHeight} cm)`
      }
    />
  );
}
