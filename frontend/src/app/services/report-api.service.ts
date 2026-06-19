import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ReportJob } from '../models/report-job';

const isProduction = window.location.hostname !== 'localhost';
const apiBase = isProduction 
  ? 'https://report-demo-worker-drishya.azurewebsites.net' 
  : 'http://localhost:5047';

@Injectable({
  providedIn: 'root'
})
export class ReportApiService {

  
  private baseUrl = `${apiBase}/api/reports`;

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

  // getMyReports() {
  //   return this.http.get<ReportJob[]>(`${this.baseUrl}/my-reports`);
  // }

  getDownloadUrl(fileUrl?: string) {
    if (!fileUrl) return '';
    if (fileUrl.startsWith('http')) return fileUrl;
    return `${apiBase}${fileUrl}`;
  }

  getNotificationCount() {
    return this.http.get<number>(
      `${this.baseUrl}/notifications/count`
    );
  }

  markNotificationsRead() {
    return this.http.post(
      `${this.baseUrl}/notifications/read`,{}
    );
  }

  getMyReports(page: number, pageSize: number) {
    return this.http.get<any>(
      `${this.baseUrl}/my-reports?page=${page}&pageSize=${pageSize}`
    );
  }

  getDashboardSummary() {
  return this.http.get<any>(
    `${this.baseUrl}/dashboard-summary`
  );
}
}