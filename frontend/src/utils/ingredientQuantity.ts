type IngredientQuantity = {
  quantity?: number;
  normalizedQuantityInGrams?: number;
  unitName?: string | null;
};

const DEFAULT_WEIGHT_UNIT = "g";

/**
 * Formats ingredient quantity using provided unit or falls back to grams when only normalized weight is available.
 */
export const formatIngredientQuantityLabel = (
  ingredient: IngredientQuantity
): string | undefined => {
  const isValidQuantity = (value: unknown): value is number =>
    typeof value === "number" && !Number.isNaN(value);

  let unit = ingredient.unitName?.trim();
  let quantity = ingredient.quantity;

  if (
    !isValidQuantity(quantity) &&
    isValidQuantity(ingredient.normalizedQuantityInGrams)
  ) {
    quantity = ingredient.normalizedQuantityInGrams;
    if (!unit) {
      unit = DEFAULT_WEIGHT_UNIT;
    }
  }

  if (!isValidQuantity(quantity) && !unit) {
    return undefined;
  }

  if (!isValidQuantity(quantity)) {
    return unit;
  }

  const rounded = Math.round(quantity * 10) / 10;
  const formattedQuantity = Number.isInteger(rounded)
    ? rounded.toFixed(0)
    : rounded.toFixed(1);
  return unit ? `${formattedQuantity} ${unit}` : formattedQuantity;
};
