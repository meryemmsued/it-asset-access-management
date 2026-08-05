import api from "../api/axios";
import type {
  Asset,
  CreateAssetRequest,
} from "../types/asset";

export async function getAssets(): Promise<Asset[]> {
  const response = await api.get<Asset[]>("/Assets");

  return response.data;
}

export async function createAsset(
  request: CreateAssetRequest
): Promise<void> {
  await api.post("/Assets", request);
}