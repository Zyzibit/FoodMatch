import { useEffect, useState } from "react";
import {
  Box,
  Typography,
  TextField,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Button,
} from "@mui/material";

type ActivityLevel = "niski" | "sredni" | "wysoki";
type Goal = "odz" | "przybranie" | "masa" | "utrzymanie";

export default function UserProfilePage() {
  const [age, setAge] = useState<number | "">("");
  const [weight, setWeight] = useState<number | "">("");
  const [height, setHeight] = useState<number | "">("");
  const [activity, setActivity] = useState<ActivityLevel | "">("");
  const [goal, setGoal] = useState<Goal | "">("");

  useEffect(() => {
    try {
      const raw = localStorage.getItem("user_profile");
      if (!raw) return;
      const parsed = JSON.parse(raw);
      if (parsed.age) setAge(parsed.age);
      if (parsed.weight) setWeight(parsed.weight);
      if (parsed.height) setHeight(parsed.height);
      if (parsed.activity) setActivity(parsed.activity);
      if (parsed.goal) setGoal(parsed.goal);
    } catch (e) {}
  }, []);

  const handleSave = () => {
    const payload = { age, weight, height, activity, goal };
    localStorage.setItem("user_profile", JSON.stringify(payload));
    alert("Dane profilu zapisane");
  };

  return (
    <Box sx={{ p: 2 }}>
      <Typography variant="h6" gutterBottom>
        Informacje użytkownika
      </Typography>
      <Box sx={{ display: "grid", gap: 2 }}>
        <TextField
          label="Wiek (lata)"
          type="number"
          value={age}
          onChange={(e) =>
            setAge(e.target.value === "" ? "" : Number(e.target.value))
          }
          inputProps={{ min: 0 }}
        />

        <TextField
          label="Waga (kg)"
          type="number"
          value={weight}
          onChange={(e) =>
            setWeight(e.target.value === "" ? "" : Number(e.target.value))
          }
          inputProps={{ min: 0 }}
        />

        <TextField
          label="Wzrost (cm)"
          type="number"
          value={height}
          onChange={(e) =>
            setHeight(e.target.value === "" ? "" : Number(e.target.value))
          }
          inputProps={{ min: 0 }}
        />

        <FormControl>
          <InputLabel id="activity-label">Poziom aktywności</InputLabel>
          <Select
            labelId="activity-label"
            label="Poziom aktywności"
            value={activity}
            onChange={(e) => setActivity(e.target.value as ActivityLevel)}
          >
            <MenuItem value={"niski"}>Niski</MenuItem>
            <MenuItem value={"sredni"}>Średni</MenuItem>
            <MenuItem value={"wysoki"}>Wysoki</MenuItem>
          </Select>
        </FormControl>

        <FormControl>
          <InputLabel id="goal-label">Cel</InputLabel>
          <Select
            labelId="goal-label"
            label="Cel"
            value={goal}
            onChange={(e) => setGoal(e.target.value as Goal)}
          >
            <MenuItem value={"odz"}>Schudnięcie</MenuItem>
            <MenuItem value={"przybranie"}>Przybranie wagi</MenuItem>
            <MenuItem value={"masa"}>Przyrost mięśni</MenuItem>
            <MenuItem value={"utrzymanie"}>Utrzymanie wagi</MenuItem>
          </Select>
        </FormControl>

        <Box sx={{ display: "flex", gap: 1, justifyContent: "flex-end" }}>
          <Button variant="contained" onClick={handleSave}>
            Zapisz
          </Button>
        </Box>
      </Box>
    </Box>
  );
}
