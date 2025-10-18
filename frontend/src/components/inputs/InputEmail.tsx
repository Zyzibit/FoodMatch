import { TextField, Box } from "@mui/material";
import { Controller } from "react-hook-form";
import type { Control } from "react-hook-form";
import { z } from "zod";
import { baseInputStyle } from "./inputStyles";
import { FieldError } from "./FieldError";
import { useFieldValidation } from "./useFieldValidation";

export const emailSchema = z
  .string()
  .min(1, "Email jest wymagany")
  .email("Niepoprawny adres email");

interface InputEmailProps {
  name?: string;
  control?: Control<any>;
  placeholder?: string;
}

export const InputEmail = ({
  name = "email",
  control,
  placeholder = "Adres e-mail",
}: InputEmailProps) => {
  if (!control) {
    const f = useFieldValidation(emailSchema);

    return (
      <Box position="relative" mt={2}>
        <TextField
          value={f.value}
          onChange={(e) => f.onChange(e.target.value)}
          onBlur={(e) => f.onBlur(e.target.value)}
          placeholder={placeholder}
          fullWidth
          margin="normal"
          variant="outlined"
          error={!!f.error}
          sx={baseInputStyle}
        />
        <FieldError message={f.error} />
      </Box>
    );
  }

  return (
    <Controller
      name={name}
      control={control}
      render={({ field }) => {
        const f = useFieldValidation(emailSchema);
        const val = (field.value ?? "") as string;

        return (
          <Box position="relative" mt={2}>
            <TextField
              value={val}
              onChange={(e) => {
                field.onChange(e);
                f.onChange(e.target.value);
              }}
              onBlur={(e) => {
                field.onBlur();
                f.onBlur(e.target.value);
              }}
              placeholder={placeholder}
              fullWidth
              margin="normal"
              variant="outlined"
              error={!!f.error}
              sx={baseInputStyle}
            />
            <FieldError message={f.error} />
          </Box>
        );
      }}
    />
  );
};
