export interface AccessRequestSummary {
  id: string;
  requestedByUserId: string;
  requestedBy: string;
  assetName: string;
  status: number;
  createdAt: string;
  canApprove: boolean;
  canCancel: boolean;
}

export interface AccessRequestDetail {
  id: string;
  assetId: string;
  assetName: string;
  requestedByUserId: string;
  requestedBy: string;
  reason: string;
  requestedStartDate: string | null;
  requestedEndDate: string | null;
  status: number;
  createdAt: string;
  approvalComment: string | null;
  decidedAt: string | null;
}

export interface CreateAccessRequestRequest {
  assetId: string;
  requestedAccessType: string;
  reason: string;
  requestedStartDate: string | null;
  requestedEndDate: string | null;
}

export interface ApproveAccessRequestRequest {
  comment: string | null;
}

export interface RejectAccessRequestRequest {
  comment: string;
}