import { API_BASE_URL } from "../config";
import { getAuthHeaders } from "./apiClient";

export interface UnitDto {
  unitId: number;
  name: string;
  description: string;
  promptDescription: string;
}

const parseUnitDto = (data: any): UnitDto => ({
  unitId: Number(data?.unitId ?? data?.UnitId ?? 0),
  name: data?.name ?? data?.Name ?? "",
  description: data?.description ?? data?.Description ?? "",
  promptDescription: data?.promptDescription ?? data?.PromptDescription ?? "",
});

export const getAllUnits = async (): Promise<UnitDto[]> => {
  const response = await fetch(`${API_BASE_URL}/units`, {
    method: "GET",
    headers: getAuthHeaders(),
    credentials: "include",
  });

  if (!response.ok) {
    const error = await response
      .json()
      .catch(() => ({ message: "Nie udało się pobrać jednostek" }));
    throw new Error(error.message || "Nie udało się pobrać jednostek");
  }

  const data = await response.json();
  if (!Array.isArray(data)) {
    return [];
  }

  return data.map(parseUnitDto);
};
