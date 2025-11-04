import { Box, Button } from "@mui/material";
import type { ReactNode } from "react";
import { useEffect, useMemo } from "react";

export type TopPanelItem = {
  key: string;
  label: string;
  icon?: ReactNode;
  disabled?: boolean;
};

type PageConfig = {
  tabs: TopPanelItem[];
  defaultTab?: string;
};

const topPanelConfigs: Record<string, PageConfig> = {
  plan: {
    tabs: [
      { key: "dzienny", label: "Dzienny" },
      { key: "tygodniowy", label: "Tygodniowy" },
    ],
    defaultTab: "dzienny",
  },
  lista: {
    tabs: [
      { key: "do-kupienia", label: "Do kupienia" },
      { key: "kupione", label: "Kupione" },
    ],
    defaultTab: "do-kupienia",
  },
  przepisy: {
    tabs: [
      { key: "moje", label: "Moje przepisy" },
      { key: "spolecznosci", label: "Przepisy społeczności" },
    ],
    defaultTab: "moje",
  },
  ustawienia: {
    tabs: [
      { key: "profil", label: "Profil" },
      { key: "bezpieczenstwo", label: "Bezpieczeństwo" },
    ],
    defaultTab: "profil",
  },
};

export default function TopPanel({
  activePage,
  activeKey,
  onChange,
  sticky = false,
}: {
  activePage: string; // ← wybór strony determinuje, jakie są zakładki
  activeKey?: string; // ← kontrola aktywnej zakładki z page
  onChange?: (key: string) => void; // ← informujemy page o kliknięciu
  sticky?: boolean; // ← opcjonalnie „przyklej” na górę
}) {
  const items = useMemo(
    () => topPanelConfigs[activePage]?.tabs ?? [],
    [activePage]
  );

  useEffect(() => {
    const conf = topPanelConfigs[activePage];
    if (!conf) return;
    const exists = items.some((t) => t.key === activeKey);
    if (!exists) {
      const fallback = conf.defaultTab ?? conf.tabs[0]?.key;
      if (fallback) onChange?.(fallback);
    }
  }, [activePage, activeKey, items, onChange]);
  return (
    <Box
      sx={(t) => ({
        width: "100%",
        bgcolor: t.palette.grey[200],
        borderBottom: `1px solid ${t.palette.grey[300]}`,
        px: 2,
        py: 1,
        position: sticky ? "sticky" : "static",
        top: sticky ? 0 : "auto",
        zIndex: sticky ? 10 : "auto",
      })}
    >
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: `repeat(${items.length}, 1fr)`, // równe kolumny
          gap: 8,
          maxWidth: 1100,
          mx: "auto",
        }}
      >
        {items.map(({ key, label, icon, disabled }) => {
          const active = key === activeKey;
          return (
            <Button
              key={key}
              onClick={() => onChange?.(key)}
              disabled={disabled}
              fullWidth
              disableElevation
              sx={(t) => ({
                borderRadius: 9999,
                py: 1,
                textTransform: "none",
                fontWeight: 700,
                justifyContent: "center",
                gap: 0.6,
                backgroundColor: active
                  ? t.palette.secondary.main
                  : t.palette.grey[300],
                color: active ? t.palette.common.white : t.palette.text.primary,
                "&:hover": {
                  backgroundColor: active
                    ? t.palette.secondary.dark
                    : t.palette.grey[400],
                },
                "&.Mui-disabled": {
                  backgroundColor: t.palette.grey[200],
                  color: t.palette.grey[500],
                },
              })}
            >
              {icon}
              {label}
            </Button>
          );
        })}
      </Box>
    </Box>
  );
}
