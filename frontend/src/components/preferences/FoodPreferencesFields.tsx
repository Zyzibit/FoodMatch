import {
  Box,
  Checkbox,
  Chip,
  FormControlLabel,
  FormGroup,
  Stack,
  Typography,
} from "@mui/material";
import { allergenOptions } from "../../constants/allergens";
import type { FoodPreferences } from "../../types/preferences";

type FoodPreferencesFieldsProps = {
  value: FoodPreferences;
  onChange: (value: FoodPreferences) => void;
};

export default function FoodPreferencesFields({
  value,
  onChange,
}: FoodPreferencesFieldsProps) {
  const update = (patch: Partial<FoodPreferences>) => {
    onChange({ ...value, ...patch });
  };

  const toggleAllergen = (allergen: string) => {
    const allergies = value.allergies.includes(allergen)
      ? value.allergies.filter((item) => item !== allergen)
      : [...value.allergies, allergen];
    update({ allergies });
  };

  return (
    <Stack spacing={2}>
      <Stack direction={{ xs: "column", md: "row" }} spacing={3}>
        <Box sx={{ flex: 1 }}>
          <Typography variant="body2" fontWeight={600} gutterBottom>
            Dieta
          </Typography>
          <FormGroup>
            <FormControlLabel
              control={
                <Checkbox
                  checked={value.isVegan}
                  onChange={(e) => update({ isVegan: e.target.checked })}
                />
              }
              label="Wegańska"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={value.isVegetarian}
                  onChange={(e) =>
                    update({ isVegetarian: e.target.checked })
                  }
                />
              }
              label="Wegetariańska"
            />
          </FormGroup>
        </Box>

        <Box sx={{ flex: 1 }}>
          <Typography variant="body2" fontWeight={600} gutterBottom>
            Nietolerancje
          </Typography>
          <FormGroup>
            <FormControlLabel
              control={
                <Checkbox
                  checked={value.hasGlutenIntolerance}
                  onChange={(e) =>
                    update({ hasGlutenIntolerance: e.target.checked })
                  }
                />
              }
              label="Gluten"
            />
            <FormControlLabel
              control={
                <Checkbox
                  checked={value.hasLactoseIntolerance}
                  onChange={(e) =>
                    update({ hasLactoseIntolerance: e.target.checked })
                  }
                />
              }
              label="Laktoza"
            />
          </FormGroup>
        </Box>
      </Stack>

      <Box>
        <Typography variant="body2" fontWeight={600} gutterBottom>
          Alergeny do pominięcia
        </Typography>
        <Stack direction="row" flexWrap="wrap" gap={1}>
          {allergenOptions.map((allergen) => {
            const isSelected = value.allergies.includes(allergen);
            return (
              <Chip
                key={allergen}
                label={allergen}
                size="small"
                color={isSelected ? "secondary" : "default"}
                variant={isSelected ? "filled" : "outlined"}
                onClick={() => toggleAllergen(allergen)}
              />
            );
          })}
        </Stack>
      </Box>
    </Stack>
  );
}
