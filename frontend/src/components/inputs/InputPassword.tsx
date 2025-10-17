import { useState } from "react";
import { TextField, IconButton, InputAdornment, Box } from "@mui/material";
import { Visibility, VisibilityOff } from "@mui/icons-material";
import { Controller } from "react-hook-form";
import type { Control } from "react-hook-form";
import { z } from "zod";
import { baseInputStyle } from "./inputStyles";
import { FieldError } from "./FieldError";
import { useFieldValidation } from "./useFieldValidation";

// ✅ Nowy schemat walidacji hasła
export const passwordSchema = z
  .string()
  .min(8, "Hasło musi mieć co najmniej 8 znaków")
  .regex(/[A-Z]/, "Hasło musi zawierać co najmniej jedną wielką literę")
  .regex(/[0-9]/, "Hasło musi zawierać co najmniej jedną cyfrę");

interface InputPasswordProps {
  name?: string;
  control?: Control<any>;
  placeholder?: string;
}

export const InputPassword = ({
  name = "password",
  control,
  placeholder = "Wpisz hasło",
}: InputPasswordProps) => {
  const [show, setShow] = useState(false);
  const toggle = () => setShow((s) => !s);

  if (!control) {
    const f = useFieldValidation(passwordSchema);

    return (
      <Box position="relative" mt={2}>
        <TextField
          value={f.value}
          onChange={(e) => f.onChange(e.target.value)}
          onBlur={(e) => f.onBlur(e.target.value)}
          type={show ? "text" : "password"}
          placeholder={placeholder}
          fullWidth
          margin="normal"
          variant="outlined"
          error={!!f.error}
          sx={baseInputStyle}
          InputProps={{
            endAdornment: (
              <InputAdornment position="end">
                <IconButton onClick={toggle} edge="end">
                  {show ? <VisibilityOff /> : <Visibility />}
                </IconButton>
              </InputAdornment>
            ),
          }}
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
        const f = useFieldValidation(passwordSchema);
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
              type={show ? "text" : "password"}
              placeholder={placeholder}
              fullWidth
              margin="normal"
              variant="outlined"
              error={!!f.error}
              sx={baseInputStyle}
              InputProps={{
                endAdornment: (
                  <InputAdornment position="end">
                    <IconButton onClick={() => setShow((s) => !s)} edge="end">
                      {show ? <VisibilityOff /> : <Visibility />}
                    </IconButton>
                  </InputAdornment>
                ),
              }}
            />
            <FieldError message={f.error} />
          </Box>
        );
      }}
    />
  );
};
