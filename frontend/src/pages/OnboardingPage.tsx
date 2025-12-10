import { useCallback, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { Paper } from "@mui/material";
import AuthLayout from "../layouts/AuthLayout";
import { type UserMeasurementsFormData } from "../components/user/UserMeasurementsForm";
import { OnboardingForm } from "../components/forms/OnboardingForm/OnboardingForm";
import userMeasurementsService from "../services/userMeasurementsService";

export default function OnboardingPage() {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);

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

  const handleSubmit = useCallback(
    async (formData: UserMeasurementsFormData) => {
      setIsLoading(true);

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
        alert(
          err instanceof Error
            ? err.message
            : "Nie udało się zapisać pomiarów. Spróbuj ponownie."
        );
      } finally {
        setIsLoading(false);
      }
    },
    [navigate]
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
        <OnboardingForm onSubmit={handleSubmit} isLoading={isLoading} />
      </Paper>
    </AuthLayout>
  );
}
