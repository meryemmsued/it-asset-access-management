import api from "../api/axios";
import type { DashboardSummary } from "../types/dashboard";

export async function getDashboardSummary(): Promise<DashboardSummary> {
  const response = await api.get<DashboardSummary>("/Dashboard");

  return response.data;
}