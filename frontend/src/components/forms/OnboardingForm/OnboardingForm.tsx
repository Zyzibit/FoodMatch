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
    <Box component="form" onSubmit={handleSubmit} noValidate width="100%">
      <Stack spacing={2.5}>
        <Box
          sx={{
            display: "flex",
            alignItems: { xs: "flex-start", sm: "center" },
            justifyContent: "space-between",
            gap: 1,
          }}
        >
          <Box>
            <Typography variant="overline" color="text.secondary">
              Kilka szybkich pytań
            </Typography>
            <Typography variant="h5" fontWeight={800} lineHeight={1.2}>
              Dane potrzebne do startu
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Dzięki nim dopasujemy plan żywieniowy do Twoich celów.
            </Typography>
          </Box>
        </Box>

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
          sx={{
            py: 1.25,
            fontWeight: 700,
            letterSpacing: 0.3,
          }}
        >
          {isLoading ? "Zapisywanie..." : "Zapisz i rozpocznij"}
        </Button>
      </Stack>
    </Box>
  );
}
