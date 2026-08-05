import api from "../api/axios";
import type { AssetCategory } from "../types/asset";

export async function getAssetCategories(): Promise<AssetCategory[]> {
  const response = await api.get<AssetCategory[]>("/AssetCategories");

  return response.data;
}