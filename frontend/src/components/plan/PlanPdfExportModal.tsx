import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Stack,
  Divider,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  CircularProgress,
} from "@mui/material";
import { Download, Close } from "@mui/icons-material";
import type { MealPlanDay } from "../../types/plan";
import { useState } from "react";
import jsPDF from "jspdf";
import html2canvas from "html2canvas";
import dietLogo from "../../assets/diet-logo.png";

type PlanPdfExportModalProps = {
  open: boolean;
  onClose: () => void;
  planData: MealPlanDay | null;
  dateLabel: string;
};

export default function PlanPdfExportModal({
  open,
  onClose,
  planData,
  dateLabel,
}: PlanPdfExportModalProps) {
  const [isGenerating, setIsGenerating] = useState(false);

  const handleDownload = async () => {
    if (!planData) return;

    setIsGenerating(true);
    try {
      const element = document.getElementById("pdf-content");
      if (!element) {
        console.error("PDF content element not found");
        return;
      }

      // Capture the element as canvas
      const canvas = await html2canvas(element, {
        scale: 2,
        useCORS: true,
        logging: false,
        backgroundColor: "#ffffff",
      });

      // Calculate PDF dimensions
      const imgWidth = 210; // A4 width in mm
      const pageHeight = 297; // A4 height in mm
      const imgHeight = (canvas.height * imgWidth) / canvas.width;
      let heightLeft = imgHeight;

      const pdf = new jsPDF("p", "mm", "a4");
      let position = 0;

      // Add first page
      pdf.addImage(
        canvas.toDataURL("image/png"),
        "PNG",
        0,
        position,
        imgWidth,
        imgHeight
      );
      heightLeft -= pageHeight;

      // Add more pages if content is longer than one page
      while (heightLeft > 0) {
        position = heightLeft - imgHeight;
        pdf.addPage();
        pdf.addImage(
          canvas.toDataURL("image/png"),
          "PNG",
          0,
          position,
          imgWidth,
          imgHeight
        );
        heightLeft -= pageHeight;
      }

      // Download the PDF
      const fileName = `plan_posilkow_${dateLabel.replace(/\s+/g, "_")}.pdf`;
      pdf.save(fileName);
    } catch (error) {
      console.error("Error generating PDF:", error);
      alert("Wystąpił błąd podczas generowania PDF");
    } finally {
      setIsGenerating(false);
    }
  };

  if (!planData) return null;

  const macroData = [
    {
      label: "Białko",
      value: planData.summary.macros.protein.value,
      target: planData.summary.macros.protein.target,
    },
    {
      label: "Tłuszcz",
      value: planData.summary.macros.fat.value,
      target: planData.summary.macros.fat.target,
    },
    {
      label: "Węglowodany",
      value: planData.summary.macros.carbs.value,
      target: planData.summary.macros.carbs.target,
    },
  ];

  return (
    <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth>
      <DialogTitle>
        <Stack
          direction="row"
          justifyContent="space-between"
          alignItems="center"
        >
          <Typography variant="h6">Podgląd planu dnia - PDF</Typography>
          <Button
            startIcon={<Close />}
            onClick={onClose}
            size="small"
            color="inherit"
          >
            Zamknij
          </Button>
        </Stack>
      </DialogTitle>

      <DialogContent>
        <Box
          id="pdf-content"
          sx={{
            bgcolor: "white",
            color: "black",
            p: 4,
            border: "1px solid #ddd",
            borderRadius: 1,
          }}
        >
          {/* Nagłówek PDF */}
          <Stack
            direction="row"
            alignItems="center"
            justifyContent="center"
            spacing={2}
            sx={{ mb: 2 }}
          >
            <Box
              component="img"
              src={dietLogo}
              alt="Diet Zynzi Logo"
              sx={{ height: 60, width: "auto" }}
            />
            <Typography variant="h4" fontWeight="bold" sx={{ color: "#000" }}>
              Diet Zynzi
            </Typography>
          </Stack>

          <Typography
            variant="h5"
            gutterBottom
            align="center"
            sx={{ mb: 1, color: "#000" }}
          >
            Plan Posiłków
          </Typography>
          <Typography
            variant="h6"
            gutterBottom
            align="center"
            sx={{ color: "#666" }}
          >
            {dateLabel}
          </Typography>

          <Divider sx={{ my: 3, borderColor: "#ddd" }} />

          {/* Podsumowanie kalorii */}
          <Box sx={{ mb: 3 }}>
            <Stack
              direction="row"
              justifyContent="center"
              alignItems="baseline"
              spacing={1}
            >
              <Typography
                variant="h3"
                fontWeight="bold"
                sx={{ color: "#1976d2" }}
              >
                {planData.consumedCalories}
              </Typography>
              <Typography variant="h5" sx={{ color: "#666" }}>
                / {planData.targetCalories} KCAL
              </Typography>
            </Stack>
            <Typography
              variant="body2"
              align="center"
              sx={{ color: "#666", mt: 0.5 }}
            >
              Spożyte / Cel dzienny
            </Typography>
          </Box>

          {/* Makroskładniki */}
          <TableContainer
            component={Paper}
            variant="outlined"
            sx={{ mb: 3, bgcolor: "white" }}
          >
            <Table size="small">
              <TableHead>
                <TableRow sx={{ bgcolor: "#f5f5f5" }}>
                  <TableCell
                    sx={{
                      fontWeight: "bold",
                      color: "#000",
                      borderBottom: "2px solid #ddd",
                    }}
                  >
                    Makroskładnik
                  </TableCell>
                  <TableCell
                    align="right"
                    sx={{
                      fontWeight: "bold",
                      color: "#000",
                      borderBottom: "2px solid #ddd",
                    }}
                  >
                    Spożyte
                  </TableCell>
                  <TableCell
                    align="right"
                    sx={{
                      fontWeight: "bold",
                      color: "#000",
                      borderBottom: "2px solid #ddd",
                    }}
                  >
                    Cel
                  </TableCell>
                  <TableCell
                    align="right"
                    sx={{
                      fontWeight: "bold",
                      color: "#000",
                      borderBottom: "2px solid #ddd",
                    }}
                  >
                    %
                  </TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {macroData.map((macro, index) => {
                  const percentage =
                    macro.target > 0
                      ? Math.round((macro.value / macro.target) * 100)
                      : 0;
                  return (
                    <TableRow
                      key={macro.label}
                      sx={{
                        bgcolor: index % 2 === 0 ? "white" : "#fafafa",
                        "&:last-child td": { border: 0 },
                      }}
                    >
                      <TableCell
                        sx={{
                          color: "#000",
                          borderBottom: "1px solid #e0e0e0",
                        }}
                      >
                        {macro.label}
                      </TableCell>
                      <TableCell
                        align="right"
                        sx={{
                          color: "#000",
                          borderBottom: "1px solid #e0e0e0",
                        }}
                      >
                        {macro.value.toFixed(1)}g
                      </TableCell>
                      <TableCell
                        align="right"
                        sx={{
                          color: "#000",
                          borderBottom: "1px solid #e0e0e0",
                        }}
                      >
                        {macro.target.toFixed(1)}g
                      </TableCell>
                      <TableCell
                        align="right"
                        sx={{
                          color: "#000",
                          borderBottom: "1px solid #e0e0e0",
                        }}
                      >
                        {percentage}%
                      </TableCell>
                    </TableRow>
                  );
                })}
              </TableBody>
            </Table>
          </TableContainer>

          {/* Lista posiłków */}
          <Typography
            variant="h6"
            fontWeight="bold"
            gutterBottom
            sx={{ mt: 3, color: "#000" }}
          >
            Zaplanowane Posiłki
          </Typography>

          {planData.meals.filter((meal) => !meal.isPlaceholder).length === 0 ? (
            <Typography
              variant="body2"
              align="center"
              sx={{ color: "#666", py: 3 }}
            >
              Brak zaplanowanych posiłków na ten dzień
            </Typography>
          ) : (
            planData.meals
              .filter((meal) => !meal.isPlaceholder)
              .map((meal, index) => (
                <Box
                  key={meal.id}
                  sx={{
                    mb: 2,
                    p: 2,
                    border: "2px solid #e0e0e0",
                    borderRadius: 2,
                    bgcolor: index % 2 === 0 ? "#f9f9f9" : "white",
                  }}
                >
                  <Stack
                    direction="row"
                    justifyContent="space-between"
                    alignItems="flex-start"
                    sx={{ mb: 1 }}
                  >
                    <Box>
                      <Typography
                        variant="h6"
                        fontWeight="bold"
                        sx={{ color: "#1976d2" }}
                      >
                        {meal.type}
                      </Typography>
                      <Typography variant="caption" sx={{ color: "#666" }}>
                        Godzina: {meal.time}
                      </Typography>
                    </Box>
                    <Box sx={{ textAlign: "right" }}>
                      <Typography
                        variant="h6"
                        fontWeight="bold"
                        sx={{ color: "#dc004e" }}
                      >
                        {meal.calories} kcal
                      </Typography>
                    </Box>
                  </Stack>

                  {meal.title && (
                    <Typography
                      variant="body1"
                      fontWeight={600}
                      sx={{ mb: 0.5, color: "#000" }}
                    >
                      {meal.title}
                    </Typography>
                  )}

                  {meal.description && (
                    <Typography variant="body2" sx={{ mb: 1, color: "#666" }}>
                      {meal.description}
                    </Typography>
                  )}

                  <Divider sx={{ my: 1, borderColor: "#ddd" }} />

                  <Typography
                    variant="caption"
                    fontWeight="bold"
                    sx={{ mb: 0.5, display: "block", color: "#666" }}
                  >
                    Makroskładniki:
                  </Typography>
                  <Stack direction="row" spacing={3}>
                    <Box>
                      <Typography
                        variant="body2"
                        fontWeight="bold"
                        sx={{ color: "#000" }}
                      >
                        Białko
                      </Typography>
                      <Typography variant="body2" sx={{ color: "#666" }}>
                        {meal.macros.protein}g
                      </Typography>
                    </Box>
                    <Box>
                      <Typography
                        variant="body2"
                        fontWeight="bold"
                        sx={{ color: "#000" }}
                      >
                        Tłuszcze
                      </Typography>
                      <Typography variant="body2" sx={{ color: "#666" }}>
                        {meal.macros.fat}g
                      </Typography>
                    </Box>
                    <Box>
                      <Typography
                        variant="body2"
                        fontWeight="bold"
                        sx={{ color: "#000" }}
                      >
                        Węglowodany
                      </Typography>
                      <Typography variant="body2" sx={{ color: "#666" }}>
                        {meal.macros.carbs}g
                      </Typography>
                    </Box>
                  </Stack>
                </Box>
              ))
          )}
        </Box>
      </DialogContent>

      <DialogActions sx={{ px: 3, pb: 2 }}>
        <Button onClick={onClose} color="inherit" disabled={isGenerating}>
          Anuluj
        </Button>
        <Button
          variant="contained"
          startIcon={
            isGenerating ? (
              <CircularProgress size={20} color="inherit" />
            ) : (
              <Download />
            )
          }
          onClick={handleDownload}
          disabled={isGenerating}
        >
          {isGenerating ? "Generowanie..." : "Pobierz PDF"}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
