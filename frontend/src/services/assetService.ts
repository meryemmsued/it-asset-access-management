import api from "../api/axios";
import type {
  Asset,
  CreateAssetRequest,
} from "../types/asset";
import type { PagedResult } from "../types/pagination";

export async function getAssets(
  page = 1,
  pageSize = 10
): Promise<PagedResult<Asset>> {
  const response = await api.get<PagedResult<Asset>>(
    "/Assets",
    {
      params: {
        page,
        pageSize,
      },
    }
  );

  return response.data;
}

export async function createAsset(
  request: CreateAssetRequest
): Promise<void> {
  await api.post("/Assets", request);
}