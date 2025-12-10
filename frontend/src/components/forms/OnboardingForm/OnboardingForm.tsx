import { useState } from "react";
import { Box, Button, Stack, Typography } from "@mui/material";
import {
  UserMeasurementsForm,
  validateMeasurements,
  type UserMeasurementsFormData,
} from "../../user/UserMeasurementsForm";

interface OnboardingFormProps {
  initialData?: UserMeasurementsFormData;
  onSubmit?: (data: UserMeasurementsFormData) => void;
  isLoading?: boolean;
}

export function OnboardingForm({
  initialData = {
    age: "",
    gender: "Male",
    weight: "",
    height: "",
    activityLevel: "ModeratelyActive",
    fitnessGoal: "Maintenance",
  },
  onSubmit,
  isLoading = false,
}: OnboardingFormProps) {
  const [formData, setFormData] =
    useState<UserMeasurementsFormData>(initialData);
  const [error, setError] = useState<string | null>(null);

  const handleFormChange = (data: UserMeasurementsFormData) => {
    setFormData(data);
    setError(null);
  };

  const handleSubmit = (event: React.FormEvent) => {
    event.preventDefault();

    const validationError = validateMeasurements(formData, true);
    if (validationError) {
      setError(validationError);
      return;
    }

    onSubmit?.(formData);
  };

  return (
    <Box
      sx={{
        p: 4,
        maxWidth: 600,
        width: "100%",
        borderRadius: 2,
      }}
    >
      <Stack spacing={3}>
        <Box textAlign="center">
          <Typography variant="h4" fontWeight={800} gutterBottom>
            Witaj w DIET ZYNZI! 🎉
          </Typography>
          <Typography variant="body1" color="text.secondary">
            Aby stworzyć spersonalizowany plan żywieniowy, potrzebujemy kilku
            informacji o Tobie.
          </Typography>
        </Box>

        <form onSubmit={handleSubmit}>
          <Stack spacing={3}>
            <UserMeasurementsForm
              initialData={formData}
              onChange={handleFormChange}
              disabled={isLoading}
              showGoal
              error={error}
              onErrorClose={() => setError(null)}
            />

            <Button
              type="submit"
              variant="contained"
              size="large"
              disabled={isLoading}
              fullWidth
            >
              {isLoading ? "Zapisywanie..." : "Zapisz i rozpocznij"}
            </Button>
          </Stack>
        </form>
      </Stack>
    </Box>
  );
}
