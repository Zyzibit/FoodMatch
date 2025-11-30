import { useState } from "react";
import { Alert, Stack } from "@mui/material";
import {
  AgeField,
  WeightField,
  HeightField,
  GenderField,
  ActivityLevelField,
  GoalField,
  type ActivityLevel,
  type FitnessGoal,
} from "../inputs";

export interface UserMeasurementsFormData {
  age: number | "";
  gender: "Male" | "Female";
  weight: number | "";
  height: number | "";
  activityLevel: ActivityLevel;
  fitnessGoal?: FitnessGoal;
}

interface UserMeasurementsFormProps {
  initialData?: Partial<UserMeasurementsFormData>;
  onChange?: (data: UserMeasurementsFormData) => void;
  disabled?: boolean;
  showGoal?: boolean;
  error?: string | null;
  onErrorClose?: () => void;
}

export function UserMeasurementsForm({
  initialData,
  onChange,
  disabled = false,
  showGoal = false,
  error,
  onErrorClose,
}: UserMeasurementsFormProps) {
  const [formData, setFormData] = useState<UserMeasurementsFormData>({
    age: initialData?.age ?? "",
    gender: initialData?.gender ?? "Male",
    weight: initialData?.weight ?? "",
    height: initialData?.height ?? "",
    activityLevel: initialData?.activityLevel ?? "ModeratelyActive",
    fitnessGoal: initialData?.fitnessGoal ?? "Maintenance",
  });

  const updateField = <K extends keyof UserMeasurementsFormData>(
    field: K,
    value: UserMeasurementsFormData[K]
  ) => {
    const updatedData = { ...formData, [field]: value };
    setFormData(updatedData);
    onChange?.(updatedData);
  };

  // Walidacja inline - zakresy zgodne z UserProfilePage
  const numericAge = typeof formData.age === "number" ? formData.age : null;
  const numericWeight =
    typeof formData.weight === "number" ? formData.weight : null;
  const numericHeight =
    typeof formData.height === "number" ? formData.height : null;

  const AGE_RANGE = { min: 7, max: 100 };
  const WEIGHT_RANGE = { min: 30, max: 250 };
  const HEIGHT_RANGE = { min: 130, max: 300 };

  const showAgeError =
    numericAge !== null &&
    (numericAge < AGE_RANGE.min || numericAge > AGE_RANGE.max);
  const showWeightError =
    numericWeight !== null &&
    (numericWeight < WEIGHT_RANGE.min || numericWeight > WEIGHT_RANGE.max);
  const showHeightError =
    numericHeight !== null &&
    (numericHeight < HEIGHT_RANGE.min || numericHeight > HEIGHT_RANGE.max);

  return (
    <Stack spacing={3}>
      {error && (
        <Alert severity="error" onClose={onErrorClose}>
          {error}
        </Alert>
      )}

      <AgeField
        value={formData.age}
        onChange={(value) => updateField("age", value)}
        disabled={disabled}
        minAge={AGE_RANGE.min}
        maxAge={AGE_RANGE.max}
        error={showAgeError}
        helperText={
          showAgeError
            ? `Wiek musi mieścić się w zakresie ${AGE_RANGE.min}-${AGE_RANGE.max} lat.`
            : undefined
        }
      />

      <GenderField
        value={formData.gender}
        onChange={(value) => updateField("gender", value)}
        disabled={disabled}
      />

      <WeightField
        value={formData.weight}
        onChange={(value) => updateField("weight", value)}
        disabled={disabled}
        minWeight={WEIGHT_RANGE.min}
        maxWeight={WEIGHT_RANGE.max}
        error={showWeightError}
        helperText={
          showWeightError
            ? `Waga musi mieścić się w zakresie ${WEIGHT_RANGE.min}-${WEIGHT_RANGE.max} kg.`
            : undefined
        }
      />

      <HeightField
        value={formData.height}
        onChange={(value) => updateField("height", value)}
        disabled={disabled}
        minHeight={HEIGHT_RANGE.min}
        maxHeight={HEIGHT_RANGE.max}
        error={showHeightError}
        helperText={
          showHeightError
            ? `Wzrost musi mieścić się w zakresie ${HEIGHT_RANGE.min}-${HEIGHT_RANGE.max} cm.`
            : undefined
        }
      />

      <ActivityLevelField
        value={formData.activityLevel}
        onChange={(value) => updateField("activityLevel", value)}
        disabled={disabled}
      />

      {showGoal && (
        <GoalField
          value={formData.fitnessGoal ?? "Maintenance"}
          onChange={(value) => updateField("fitnessGoal", value)}
          disabled={disabled}
          showDetails
        />
      )}
    </Stack>
  );
}

export function validateMeasurements(
  data: UserMeasurementsFormData,
  requireGoal = false
): string | null {
  const AGE_RANGE = { min: 7, max: 100 };
  const WEIGHT_RANGE = { min: 30, max: 250 };
  const HEIGHT_RANGE = { min: 130, max: 300 };

  if (!data.age || data.age < AGE_RANGE.min || data.age > AGE_RANGE.max) {
    return `Podaj prawidłowy wiek (${AGE_RANGE.min}-${AGE_RANGE.max} lat)`;
  }
  if (
    !data.weight ||
    data.weight < WEIGHT_RANGE.min ||
    data.weight > WEIGHT_RANGE.max
  ) {
    return `Podaj prawidłową wagę (${WEIGHT_RANGE.min}-${WEIGHT_RANGE.max} kg)`;
  }
  if (
    !data.height ||
    data.height < HEIGHT_RANGE.min ||
    data.height > HEIGHT_RANGE.max
  ) {
    return `Podaj prawidłowy wzrost (${HEIGHT_RANGE.min}-${HEIGHT_RANGE.max} cm)`;
  }
  if (!data.gender) {
    return "Wybierz płeć";
  }
  if (!data.activityLevel) {
    return "Wybierz poziom aktywności";
  }
  if (requireGoal && !data.fitnessGoal) {
    return "Wybierz swój cel fitness";
  }
  return null;
}
