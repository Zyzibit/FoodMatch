import { describe, expect, it } from "vitest";
import { formatIngredientQuantityLabel } from "../utils/ingredientQuantity";

describe("formatIngredientQuantityLabel", () => {
  it("falls back to normalized grams when quantity is missing", () => {
    const label = formatIngredientQuantityLabel({
      normalizedQuantityInGrams: 12.34,
    });
    expect(label).toBe("12.3 g");
  });

  it("returns unit when only unit is provided", () => {
    const label = formatIngredientQuantityLabel({ unitName: "ml" });
    expect(label).toBe("ml");
  });

  it("formats integer quantities without decimals", () => {
    const label = formatIngredientQuantityLabel({
      quantity: 3,
      unitName: "szt",
    });
    expect(label).toBe("3 szt");
  });
});
