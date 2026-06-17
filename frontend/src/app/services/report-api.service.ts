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

  mockComplete(jobId: string) {
    return this.http.post<ReportJob>(
      `${this.baseUrl}/mock-complete/${jobId}`,
      {}
    );
  }
}