import { Fragment } from "react";
import { TextField, Box } from "@mui/material";
import { Controller } from "react-hook-form";
import type { Control } from "react-hook-form";
import { z } from "zod";
import { baseInputStyle } from "./inputStyles";
import { FieldError } from "./FieldError";
import { useFieldValidation } from "./useFieldValidation";

export const loginSchema = z
  .string()
  .min(3, "Login musi mieć co najmniej 3 znaki")
  .max(20, "Login może mieć maksymalnie 20 znaków")
  .regex(/^[a-zA-Z0-9._-]+$/, "Dozwolone: litery, cyfry, ., _, -");

interface InputLoginProps {
  name?: string;
  control?: Control<any>;
  placeholder?: string;
}

export const InputLogin = ({
  name = "login",
  control,
  placeholder = "Nazwa użytkownika",
}: InputLoginProps) => {
  if (!control) {
    const f = useFieldValidation(loginSchema);

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
        const f = useFieldValidation(loginSchema);
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
