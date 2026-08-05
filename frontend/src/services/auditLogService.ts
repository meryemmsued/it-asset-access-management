import api from "../api/axios";
import type { AuditLog } from "../types/auditLog";
import type { PagedResult } from "../types/pagination";

export async function getAuditLogs(
  page = 1,
  pageSize = 10
): Promise<PagedResult<AuditLog>> {
  const response = await api.get<PagedResult<AuditLog>>(
    "/AuditLogs",
    {
      params: {
        page,
        pageSize,
      },
    }
  );

  return response.data;
}

export async function getAuditLogById(
  id: string
): Promise<AuditLog> {
  const response = await api.get<AuditLog>(
    `/AuditLogs/${id}`
  );

  return response.data;
}