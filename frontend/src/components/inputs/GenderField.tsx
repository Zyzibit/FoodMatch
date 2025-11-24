import {
  FormControl,
  FormControlLabel,
  FormLabel,
  Radio,
  RadioGroup,
} from "@mui/material";

interface GenderFieldProps {
  value: "Male" | "Female" | "";
  onChange: (value: "Male" | "Female") => void;
  disabled?: boolean;
}

export function GenderField({
  value,
  onChange,
  disabled = false,
}: GenderFieldProps) {
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    onChange(e.target.value as "Male" | "Female");
  };

  return (
    <FormControl component="fieldset" required disabled={disabled}>
      <FormLabel component="legend">Płeć</FormLabel>
      <RadioGroup row value={value} onChange={handleChange}>
        <FormControlLabel value="Male" control={<Radio />} label="Mężczyzna" />
        <FormControlLabel value="Female" control={<Radio />} label="Kobieta" />
      </RadioGroup>
    </FormControl>
  );
}
