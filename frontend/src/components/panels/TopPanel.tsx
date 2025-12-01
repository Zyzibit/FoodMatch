import { ChevronLeft, ChevronRight } from "@mui/icons-material";
import { Box } from "@mui/material";
import type { ReactNode } from "react";
import { useEffect, useMemo, useState } from "react";
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
    tabs: [{ key: "do-kupienia", label: "Do kupienia" }],
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
      { key: "profil", label: "Profil" },
      { key: "pomiary", label: "Pomiary" },
      { key: "zapotrzebowanie", label: "Zapotrzebowanie" },
      { key: "preferencje", label: "Preferencje" },
    ],
    defaultTab: "pomiary",
  },
};

const DATE_KEY_PREFIX = "date-";
const DATE_WINDOW_LENGTH = 9;
const DATE_WINDOW_PADDING = 2;
const DAY_IN_MS = 24 * 60 * 60 * 1000;
const NAV_OVERLAY_WIDTH = 28;

const getToday = () => {
  const today = new Date();
  today.setHours(0, 0, 0, 0);
  return today;
};

const addDays = (date: Date, amount: number) => {
  const copy = new Date(date);
  copy.setDate(copy.getDate() + amount);
  copy.setHours(0, 0, 0, 0);
  return copy;
};

const formatDateKey = (date: Date) => {
  const year = date.getFullYear();
  const month = `${date.getMonth() + 1}`.padStart(2, "0");
  const day = `${date.getDate()}`.padStart(2, "0");
  return `${DATE_KEY_PREFIX}${year}-${month}-${day}`;
};

const formatTileLabel = (date: Date, todayTime: number) => {
  const weekday = date
    .toLocaleDateString("pl-PL", { weekday: "short" })
    .replace(".", "");
  const day = date.getDate().toString().padStart(2, "0");
  const isToday = date.getTime() === todayTime;
  return isToday ? `Dziś ${day}` : `${weekday} ${day}`;
};

const parseDateKey = (key?: string) => {
  if (!key?.startsWith(DATE_KEY_PREFIX)) return null;
  const iso = key.slice(DATE_KEY_PREFIX.length);
  const [yearStr, monthStr, dayStr] = iso.split("-");
  const year = Number(yearStr);
  const month = Number(monthStr);
  const day = Number(dayStr);
  if (!year || !month || !day) return null;
  const parsed = new Date(year, month - 1, day);
  return Number.isNaN(parsed.getTime()) ? null : parsed;
};

const differenceInDays = (from: Date, to: Date) =>
  Math.round((to.getTime() - from.getTime()) / DAY_IN_MS);

const ensureDateWithinWindow = (windowStart: Date, targetDate: Date) => {
  const minIndex = DATE_WINDOW_PADDING;
  const maxIndex = DATE_WINDOW_LENGTH - 1 - DATE_WINDOW_PADDING;
  const diff = differenceInDays(windowStart, targetDate);

  if (diff < minIndex) {
    return addDays(targetDate, -minIndex);
  }
  if (diff > maxIndex) {
    return addDays(targetDate, -maxIndex);
  }
  return windowStart;
};

const centerWindowAround = (date: Date) => addDays(date, -DATE_WINDOW_PADDING);

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
  const isPlanPage = activePage === "plan";
  const [dateWindowStart, setDateWindowStart] = useState<Date>(() =>
    addDays(getToday(), -DATE_WINDOW_PADDING)
  );
  // date strip: create sliding window that can extend infinitely
  const todayTime = getToday().getTime();
  const todayDate = useMemo(() => {
    const d = new Date(todayTime);
    d.setHours(0, 0, 0, 0);
    return d;
  }, [todayTime]);
  const todayKey = formatDateKey(todayDate);

  const dateTiles = useMemo(() => {
    if (!isPlanPage) return [];

    const tiles = Array.from({ length: DATE_WINDOW_LENGTH }, (_, index) => {
      const d = addDays(dateWindowStart, index);
      return {
        key: formatDateKey(d),
        label: formatTileLabel(d, todayTime),
      };
    });

    const windowEnd = addDays(dateWindowStart, DATE_WINDOW_LENGTH - 1);

    if (todayDate < dateWindowStart) {
      tiles[0] = {
        key: todayKey,
        label: formatTileLabel(todayDate, todayTime),
      };
    } else if (todayDate > windowEnd) {
      tiles[tiles.length - 1] = {
        key: todayKey,
        label: formatTileLabel(todayDate, todayTime),
      };
    }

    return tiles;
  }, [dateWindowStart, isPlanPage, todayDate, todayKey, todayTime]);

  const items = useMemo(
    () => topPanelConfigs[activePage]?.tabs ?? [],
    [activePage]
  );

  useEffect(() => {
    // when switching to plan, set today's date as default active if none set
    if (isPlanPage) {
      const activeDate = parseDateKey(activeKey);
      if (!activeDate) {
        const todayKey = formatDateKey(getToday());
        if (activeKey !== todayKey) onChange?.(todayKey);
      }
      return;
    }

    const conf = topPanelConfigs[activePage];
    if (!conf) return;
    const exists = items.some((t) => t.key === activeKey);
    if (!exists) {
      const fallback = conf.defaultTab ?? conf.tabs[0]?.key;
      if (fallback) onChange?.(fallback);
    }
  }, [activePage, activeKey, onChange, isPlanPage]);

  useEffect(() => {
    if (!isPlanPage) return;
    const targetDate = parseDateKey(activeKey) ?? getToday();
    setDateWindowStart((current) => {
      const nextStart = ensureDateWithinWindow(current, targetDate);
      if (nextStart.getTime() === current.getTime()) return current;
      return nextStart;
    });
  }, [activeKey, isPlanPage]);

  const handleDateWindowShift = (offset: number) => {
    setDateWindowStart((current) => addDays(current, offset));
  };

  const handleTileClick = (key: string) => {
    if (isPlanPage && key === todayKey) {
      setDateWindowStart(centerWindowAround(todayDate));
    }
    onChange?.(key);
  };

  const renderArray = isPlanPage ? dateTiles : items;
  const count = renderArray.length;

  return (
    <Box
      sx={(t) => ({
        width: "100%",
        bgcolor: t.palette.background.paper,
        borderBottom: `1px solid ${t.palette.divider}`,
        px: 0,
        py: 0, // no vertical padding so inner strip can control exact height
        position: sticky ? "sticky" : "static",
        top: sticky ? 0 : "auto",
        zIndex: sticky ? 10 : "auto",
      })}
    >
      <Box
        sx={{
          position: "relative",
          width: "100%",
          height: TOP_PANEL_HEIGHT,
          overflow: "hidden",
          px: 0,
        }}
      >
        <Box
          sx={{
            display: "grid",
            gridTemplateColumns: `repeat(${count}, 1fr)`,
            gap: 0,
            width: "100%",
            height: "100%",
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
                    i < count - 1 ? `1px solid ${t.palette.divider}` : "none",
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
                  onClick={() => handleTileClick(key)}
                />
              </Box>
            );
          })}
        </Box>
        {isPlanPage && (
          <>
            <Box
              component="button"
              type="button"
              aria-label="Poprzednie dni"
              onClick={() => handleDateWindowShift(-1)}
              sx={(t) => ({
                position: "absolute",
                top: 0,
                bottom: 0,
                left: 0,
                width: NAV_OVERLAY_WIDTH,
                border: "none",
                padding: 0,
                outline: "none",
                appearance: "none",
                cursor: "pointer",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                backgroundColor: "transparent",
                color: t.palette.text.secondary,
                zIndex: 2,
                transition: "color 0.2s ease, background-color 0.2s ease",
                "&:hover": {
                  color: t.palette.secondary.contrastText,
                  backgroundColor: t.palette.secondary.main,
                },
                "&:focus-visible": {
                  outline: `2px solid ${t.palette.secondary.main}`,
                  outlineOffset: -2,
                },
                "& > svg": { fontSize: 22 },
              })}
            >
              <ChevronLeft />
            </Box>
            <Box
              component="button"
              type="button"
              aria-label="Kolejne dni"
              onClick={() => handleDateWindowShift(1)}
              sx={(t) => ({
                position: "absolute",
                top: 0,
                bottom: 0,
                right: 0,
                width: NAV_OVERLAY_WIDTH,
                border: "none",
                padding: 0,
                outline: "none",
                appearance: "none",
                cursor: "pointer",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                backgroundColor: "transparent",
                color: t.palette.text.secondary,
                zIndex: 2,
                transition: "color 0.2s ease, background-color 0.2s ease",
                "&:hover": {
                  color: t.palette.secondary.contrastText,
                  backgroundColor: t.palette.secondary.main,
                },
                "&:focus-visible": {
                  outline: `2px solid ${t.palette.secondary.main}`,
                  outlineOffset: -2,
                },
                "& > svg": { fontSize: 22 },
              })}
            >
              <ChevronRight />
            </Box>
          </>
        )}
      </Box>
    </Box>
  );
}
