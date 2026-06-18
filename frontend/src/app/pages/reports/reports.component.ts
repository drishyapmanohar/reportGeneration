import { Component, OnInit, effect, computed, signal } from '@angular/core';
import { ReportPollingService } from '../../services/report-polling.service';
import { ReportSignalrService } from '../../services/report-signalr.service';
import { ReportApiService } from '../../services/report-api.service';
import { DatePipe } from '@angular/common';

@Component({
  selector: 'app-reports',
  standalone: true,
  imports: [DatePipe],
  templateUrl: './reports.component.html',
  styleUrl: './reports.component.scss'
})
export class ReportsComponent implements OnInit {
  page = signal(1);
  pageSize = 10;
  dashboard = signal({
    totalReports: 0,
    completedReports: 0,
    inProgressReports: 0,
    failedReports: 0
  });
  constructor(
    public polling: ReportPollingService,
    public api: ReportApiService,
    public signalr: ReportSignalrService
  ) {
    effect(() => {
      const updatedJob = this.signalr.reportUpdated();

      if (updatedJob) {

        this.polling.loadReports(this.page(), this.pageSize);
        this.loadDashboardSummary();

        // fallback check
        if (updatedJob.status === 'InProgress') {
          setTimeout(() => {
            this.polling.loadReports(this.page(), this.pageSize);
            this.loadDashboardSummary();
          }, 15000);
        }
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit() {
    this.polling.loadReports(this.page(), this.pageSize);
    this.loadDashboardSummary();

    this.api.markNotificationsRead()
      .subscribe(() => {
        this.signalr.notificationCount.set(0);
      });

    document.addEventListener('visibilitychange', () => {
      if (!document.hidden) {
        this.polling.loadReports(this.page(), this.pageSize);
      }
    });
  }

  totalReports() {
    return this.dashboard().totalReports;
  }

  completedReports() {
    return this.dashboard().completedReports;
  }

  inProgressReports() {
    return this.dashboard().inProgressReports;
  }

  failedReports() {
    return this.dashboard().failedReports;
  }

  nextPage() {
    if (this.page() < this.polling.totalPages()) {
      this.page.update(p => p + 1);
      this.polling.loadReports(this.page(), this.pageSize);
    }
  }

  previousPage() {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
      this.polling.loadReports(this.page(), this.pageSize);
    }
  }

  totalPages() {
    return this.polling.totalPages();
  }

  pagedJobs() {
    return this.polling.jobs();
  }

  loadDashboardSummary() {
    this.api.getDashboardSummary().subscribe(res => {
      this.dashboard.set(res);
    });
  }
}