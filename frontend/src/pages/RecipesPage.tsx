import { Paper, Typography } from "@mui/material";
import { useDashboardContext } from "../layouts/DashboardLayout";

const tabLabels: Record<string, string> = {
  moje: "Moje przepisy",
  "spolecznosci": "Przepisy społeczności",
};

export default function RecipesPage() {
  const { activeTab } = useDashboardContext();
  const label = activeTab ? tabLabels[activeTab] ?? activeTab : "Moje przepisy";

  return (
    <Paper
      elevation={1}
      sx={{ p: 3, width: "100%", maxWidth: 1100, mx: "auto" }}
    >
      <Typography variant="h5" gutterBottom fontWeight={800}>
        Przepisy
      </Typography>
      <Typography variant="subtitle1" gutterBottom>
        Zakładka: {label}
      </Typography>
      <Typography variant="body2" color="text.secondary">
        Tu pojawi się lista przepisów wraz z filtrowaniem po wybranej zakładce.
      </Typography>
    </Paper>
  );
}
