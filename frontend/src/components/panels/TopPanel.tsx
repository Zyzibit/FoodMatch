import { Box } from "@mui/material";
import type { ReactNode } from "react";
import { useEffect, useMemo } from "react";
import Tile from "../buttons/Tile";

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
  user: {
    tabs: [
      { key: "pomiary", label: "Pomiary" },
      { key: "zapotrzebowanie", label: "Zapotrzebowanie" },
      { key: "alergeny", label: "Alergeny" },
    ],
    defaultTab: "pomiary",
  },
};

export default function TopPanel({
  activePage,
  activeKey,
  onChange,
  sticky = false,
}: {
  activePage: string;
  activeKey?: string;
  onChange?: (key: string) => void;
  sticky?: boolean;
}) {
  const TOP_PANEL_HEIGHT = 56; // px - fixed height for the strip so tiles always same height
  // date strip: always create 9 days from -2..+6 relative to today
  const dateTiles = useMemo(() => {
    const today = new Date();
    const start = new Date(today);
    start.setDate(today.getDate() - 2);
    const days: { key: string; label: string }[] = [];
    const total = 9; // -2..+6
    for (let i = 0; i < total; i++) {
      const d = new Date(start);
      d.setDate(start.getDate() + i);
      const iso = d.toISOString().slice(0, 10);
      const weekday = d
        .toLocaleDateString("pl-PL", { weekday: "short" })
        .replace(".", "");
      const day = d.getDate().toString().padStart(2, "0");
      days.push({ key: `date-${iso}`, label: `${weekday} ${day}` });
    }
    return days;
  }, []);

  const items = useMemo(
    () => topPanelConfigs[activePage]?.tabs ?? [],
    [activePage]
  );

  useEffect(() => {
    // when switching to plan, set today's date as default active if none set
    if (activePage === "plan") {
      const today = new Date();
      const iso = today.toISOString().slice(0, 10);
      const todayKey = `date-${iso}`;
      const exists = dateTiles.some((t) => t.key === activeKey);
      if (!exists) onChange?.(todayKey);
      return;
    }

    const conf = topPanelConfigs[activePage];
    if (!conf) return;
    const exists = items.some((t) => t.key === activeKey);
    if (!exists) {
      const fallback = conf.defaultTab ?? conf.tabs[0]?.key;
      if (fallback) onChange?.(fallback);
    }
  }, [activePage, activeKey, items, onChange, dateTiles]);

  const renderArray = activePage === "plan" ? dateTiles : items;
  const count = renderArray.length;

  return (
    <Box
      sx={(t) => ({
        width: "100%",
        bgcolor: t.palette.grey[200],
        borderBottom: `1px solid ${t.palette.grey[300]}`,
        px: 0,
        py: 0, // no vertical padding so inner strip can control exact height
        position: sticky ? "sticky" : "static",
        top: sticky ? 0 : "auto",
        zIndex: sticky ? 10 : "auto",
      })}
    >
      <Box
        sx={{
          display: "grid",
          gridTemplateColumns: `repeat(${count}, 1fr)`,
          gap: 0,
          width: "100%",
          height: TOP_PANEL_HEIGHT,
          alignItems: "stretch",
        }}
      >
        {renderArray.map((it, i) => {
          const key = (it as any).key as string;
          const label = (it as any).label as string;
          const icon = (it as any).icon as ReactNode | undefined;
          const disabled = (it as any).disabled as boolean | undefined;
          const active = key === activeKey;

          return (
            <Box
              key={key}
              sx={(t) => ({
                width: "100%",
                height: "100%",
                borderRight:
                  i < count - 1 ? `1px solid ${t.palette.grey[300]}` : "none",
              })}
            >
              <Tile
                title={label}
                icon={icon}
                size={activePage === "plan" ? "sm" : "md"}
                square
                fullHeight
                active={active}
                disabled={disabled}
                onClick={() => onChange?.(key)}
              />
            </Box>
          );
        })}
      </Box>
    </Box>
  );
}
