export type FitnessGoal = "WeightLoss" | "Maintenance" | "WeightGain";

export interface FitnessGoalOption {
  value: FitnessGoal;
  label: string;
  description: string;
  adjustmentNote: string;
}

export const FITNESS_GOAL_OPTIONS: FitnessGoalOption[] = [
  {
    value: "WeightLoss",
    label: "Schudnąć (deficyt kalorii)",
    description:
      "Dostarczaj ok. 20% mniej kalorii niż Twoje całkowite zapotrzebowanie.",
    adjustmentNote: "-20% kalorii · wyższe białko",
  },
  {
    value: "Maintenance",
    label: "Utrzymać wagę",
    description:
      "Jedz tyle, ile wynosi Twoje dzienne zapotrzebowanie energetyczne.",
    adjustmentNote: "0% zmian · zbilansowane makro",
  },
  {
    value: "WeightGain",
    label: "Przybrać na masie",
    description:
      "Dodaj ok. 15% dodatkowych kalorii, aby wspierać budowę masy.",
    adjustmentNote: "+15% kalorii · więcej białka i węgli",
  },
];
