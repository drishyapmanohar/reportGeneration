import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReportJob } from '../models/report-job';

@Injectable({
  providedIn: 'root'
})
export class ReportApiService {
  private baseUrl = 'http://localhost:5047/api/reports';

  constructor(private http: HttpClient) {}

  generateReport() {
    return this.http.post<{ jobId: string; message: string }>(
      `${this.baseUrl}/generate`,
      {}
    );
  }

  getStatus(jobId: string) {
    return this.http.get<ReportJob>(`${this.baseUrl}/status/${jobId}`);
  }

  getMyReports() {
    return this.http.get<ReportJob[]>(`${this.baseUrl}/my-reports`);
  }

  getDownloadUrl(fileUrl?: string) {
    if (!fileUrl) return '';
    if (fileUrl.startsWith('http')) return fileUrl;
    return `http://localhost:5047${fileUrl}`;
  }

  getNotificationCount() {
    return this.http.get<number>(
      `${this.baseUrl}/notifications/count`
    );
  }

  markNotificationsRead() {
    return this.http.post(
      'http://localhost:5047/api/reports/notifications/read',
      {}
    );
  }
}