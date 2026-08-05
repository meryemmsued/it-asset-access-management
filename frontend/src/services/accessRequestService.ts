import api from "../api/axios";

import type {
  AccessRequestDetail,
  AccessRequestSummary,
  CreateAccessRequestRequest,
} from "../types/accessRequest";

export async function getAllAccessRequests():
Promise<AccessRequestSummary[]> {
  const response = await api.get<AccessRequestSummary[]>(
    "/AccessRequests"
  );

  return response.data;
}

export async function getMyAccessRequests():
Promise<AccessRequestSummary[]> {
  const response = await api.get<AccessRequestSummary[]>(
    "/AccessRequests/my"
  );

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
  comment: string
): Promise<void> {
  await api.post(`/AccessRequests/${id}/approve`, {
    comment,
  });
}

export async function rejectAccessRequest(
  id: string,
  comment: string
): Promise<void> {
  await api.post(`/AccessRequests/${id}/reject`, {
    comment,
  });
}

export async function cancelAccessRequest(
  id: string
): Promise<void> {
  await api.post(`/AccessRequests/${id}/cancel`);
}