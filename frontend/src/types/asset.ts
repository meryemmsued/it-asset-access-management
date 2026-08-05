export interface Asset {
  id: string;
  assetCode: string;
  name: string;
  category: string;
  status: number;
}

export interface CreateAssetRequest {
  assetCategoryId: string;
  assetCode: string;
  name: string;
  description: string;
  purchaseDate: string | null;
  purchasePrice: number | null;
  warrantyExpirationDate: string | null;

  serialNumber: string | null;
  manufacturer: string | null;
  model: string | null;
  location: string | null;
  condition: number | null;

  licenseKey: string | null;
  version: string | null;
  licenseType: number | null;
  licenseStartDate: string | null;
  licenseExpirationDate: string | null;
  requestedAccessType: string | null;
  maximumUsers: number | null;
}

export interface AssetCategory {
  id: string;
  name: string;
  assetType: "Physical" | "Digital";
}