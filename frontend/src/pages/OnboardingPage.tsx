import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Box, Button, Paper, Stack, Typography } from "@mui/material";
import AuthLayout from "../layouts/AuthLayout";
import {
  UserMeasurementsForm,
  validateMeasurements,
  type UserMeasurementsFormData,
} from "../components/user/UserMeasurementsForm";
import userMeasurementsService from "../services/userMeasurementsService";

export default function OnboardingPage() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formData, setFormData] = useState<UserMeasurementsFormData>({
    age: "",
    gender: "Male",
    weight: "",
    height: "",
    activityLevel: "ModeratelyActive",
    fitnessGoal: "Maintenance",
  });

  // Sprawdź czy użytkownik już ma zapisane pomiary
  useEffect(() => {
    const checkExistingMeasurements = async () => {
      try {
        const hasMeasurements = await userMeasurementsService.hasMeasurements();
        if (hasMeasurements) {
          // Użytkownik już ma pomiary, przekieruj do dashboardu
          navigate("/app/plan", { replace: true });
        }
      } catch (err) {
        // Błąd lub brak pomiarów - pozostań na stronie onboardingu
        console.log("No existing measurements found");
      }
    };

    checkExistingMeasurements();
  }, [navigate]);

  const handleFormChange = (data: UserMeasurementsFormData) => {
    setFormData(data);
    setError(null);
  };

  const handleSubmit = useCallback(
    async (event: React.FormEvent) => {
      event.preventDefault();

      const validationError = validateMeasurements(formData, true);
      if (validationError) {
        setError(validationError);
        return;
      }

      setIsLoading(true);
      setError(null);

      try {
        await userMeasurementsService.updatePreferences({
          age: formData.age as number,
          gender: formData.gender,
          weight: formData.weight as number,
          height: formData.height as number,
          activityLevel: formData.activityLevel,
          fitnessGoal: formData.fitnessGoal,
        });

        // Po zapisaniu pomiarów przekieruj do dashboardu
        navigate("/app/plan", { replace: true });
      } catch (err) {
        console.error("Error saving measurements:", err);
        setError(
          err instanceof Error
            ? err.message
            : "Nie udało się zapisać pomiarów. Spróbuj ponownie."
        );
      } finally {
        setIsLoading(false);
      }
    },
    [formData, navigate]
  );

  return (
    <AuthLayout title="DIET ZYNZI">
      <Paper
        elevation={3}
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
      </Paper>
    </AuthLayout>
  );
}
