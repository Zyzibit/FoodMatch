import { createContext, useContext } from "react";
import type { ThemeName } from "../theme";

export type AppThemeContextValue = {
  themeName: ThemeName;
  setThemeName: (name: ThemeName) => void;
};

const AppThemeContext = createContext<AppThemeContextValue | undefined>(
  undefined
);

export const useAppTheme = () => {
  const ctx = useContext(AppThemeContext);
  if (!ctx) {
    throw new Error("useAppTheme must be used within AppThemeContext.Provider");
  }
  return ctx;
};

export default AppThemeContext;
