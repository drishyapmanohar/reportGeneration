import { Component } from '@angular/core';
import { ReportApiService } from '../../services/report-api.service';

@Component({
  selector: 'app-generate-report',
  standalone: true,
  templateUrl: './generate-report.component.html',
  styleUrl: './generate-report.component.scss'
})
export class GenerateReportComponent {
  loading = false;
  message = '';

  constructor(private api: ReportApiService) {}

  generateReport() {
    this.loading = true;
    this.message = '';

    this.api.generateReport().subscribe({
      next: res => {
        this.loading = false;
        this.message = `Report submitted. Job ID: ${res.jobId}`;
      },
      error: () => {
        this.loading = false;
        this.message = 'Failed to submit report.';
      }
    });
  }
}