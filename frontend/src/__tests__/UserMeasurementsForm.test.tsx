import type { ComponentProps } from "react";
import { render, screen, waitFor } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ThemeProvider, createTheme } from "@mui/material/styles";
import { vi } from "vitest";
import { UserMeasurementsForm } from "../components/user/UserMeasurementsForm";

const renderForm = (props?: ComponentProps<typeof UserMeasurementsForm>) =>
  render(
    <ThemeProvider theme={createTheme()}>
      <UserMeasurementsForm showGoal {...props} />
    </ThemeProvider>
  );

describe("UserMeasurementsForm", () => {
  it("calls onChange with numeric values when the user types", async () => {
    const user = userEvent.setup();
    const onChange = vi.fn();
    renderForm({ onChange });

    const ageInput = screen.getByLabelText(/Wiek/i);
    await user.clear(ageInput);
    await user.type(ageInput, "32");

    await waitFor(() => expect(onChange).toHaveBeenCalled());
    const latestCall = onChange.mock.lastCall?.[0];
    expect(latestCall?.age).toBe(32);
    expect(latestCall?.gender).toBe("Male");
  });

  it("shows inline validation when weight is out of range", async () => {
    const user = userEvent.setup();
    renderForm();

    const weightInput = screen.getByLabelText(/Waga/i);
    await user.clear(weightInput);
    await user.type(weightInput, "10");

    expect(
      await screen.findByText(/Waga musi mieścić się w zakresie 30-250 kg/i)
    ).toBeInTheDocument();
    await waitFor(() =>
      expect(weightInput).toHaveAttribute("aria-invalid", "true")
    );
  });
});
