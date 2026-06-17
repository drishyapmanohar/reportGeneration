export interface ReportJob {
  id: string;
  reportType: string;
  status: 'Pending' | 'InProgress' | 'Completed' | 'Failed';
  fileUrl?: string;
  createdAt: string;
  completedAt?: string;
}