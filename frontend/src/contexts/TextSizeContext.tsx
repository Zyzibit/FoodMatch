import { createContext, useContext } from "react";

export type TextSize = "sm" | "md" | "lg";

type TextSizeContextValue = {
  textSize: TextSize;
  setTextSize: (size: TextSize) => void;
};

const TextSizeContext = createContext<TextSizeContextValue | undefined>(
  undefined
);

export const useTextSize = () => {
  const context = useContext(TextSizeContext);
  if (!context) {
    throw new Error("useTextSize must be used within TextSizeContext.Provider");
  }
  return context;
};

export default TextSizeContext;
