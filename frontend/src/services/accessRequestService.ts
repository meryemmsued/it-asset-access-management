import api from "../api/axios";

import type {
  AccessRequestDetail,
  AccessRequestSummary,
  ApproveAccessRequestRequest,
  CreateAccessRequestRequest,
  RejectAccessRequestRequest,
} from "../types/accessRequest";

import type { PagedResult } from "../types/pagination";

export async function getAllAccessRequests(
  page = 1,
  pageSize = 10
): Promise<PagedResult<AccessRequestSummary>> {
  const response = await api.get<
    PagedResult<AccessRequestSummary>
  >("/AccessRequests", {
    params: {
      page,
      pageSize,
    },
  });

  return response.data;
}

export async function getMyAccessRequests(
  page = 1,
  pageSize = 10
): Promise<PagedResult<AccessRequestSummary>> {
  const response = await api.get<
    PagedResult<AccessRequestSummary>
  >("/AccessRequests/my", {
    params: {
      page,
      pageSize,
    },
  });

  return response.data;
}

export async function getAccessRequestById(
  id: string
): Promise<AccessRequestDetail> {
  const response = await api.get<AccessRequestDetail>(
    `/AccessRequests/${id}`
  );

  return response.data;
}

export async function createAccessRequest(
  request: CreateAccessRequestRequest
): Promise<AccessRequestDetail> {
  const response = await api.post<AccessRequestDetail>(
    "/AccessRequests",
    request
  );

  return response.data;
}

export async function approveAccessRequest(
  id: string,
  request: ApproveAccessRequestRequest
): Promise<void> {
  await api.post(
    `/AccessRequests/${id}/approve`,
    request
  );
}

export async function rejectAccessRequest(
  id: string,
  request: RejectAccessRequestRequest
): Promise<void> {
  await api.post(
    `/AccessRequests/${id}/reject`,
    request
  );
}

export async function cancelAccessRequest(
  id: string
): Promise<void> {
  await api.post(
    `/AccessRequests/${id}/cancel`
  );
}