export interface AuditLog {
  id: string;
  userId: string | null;
  action: string;
  entityType: string;
  entityId: string | null;
  oldValues: string | null;
  newValues: string | null;
  ipAddress: string | null;
  userAgent: string | null;
  createdAt: string;
}

export interface DashboardSummary {
  totalUsers: number;
  activeUsers: number;
  totalAssets: number;
  availableAssets: number;
  assignedAssets: number;
  pendingAccessRequests: number;
  approvedAccessRequests: number;
  rejectedAccessRequests: number;
  recentAuditLogs: AuditLog[];
}