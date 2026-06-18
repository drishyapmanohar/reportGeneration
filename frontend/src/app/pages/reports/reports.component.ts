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

  pagedJobs() {
    const start = (this.page() - 1) * this.pageSize;
    return this.polling.jobs().slice(start, start + this.pageSize);
  }

  totalPages() {
    return Math.ceil(this.polling.jobs().length / this.pageSize) || 1;
  }

  nextPage() {
    if (this.page() < this.totalPages()) {
      this.page.update(p => p + 1);
    }
  }

  previousPage() {
    if (this.page() > 1) {
      this.page.update(p => p - 1);
    }
  }

  constructor(
    public polling: ReportPollingService,
    public api: ReportApiService,
    private signalr: ReportSignalrService
  ) {
    effect(() => {
      const updatedJob = this.signalr.reportUpdated();

      if (updatedJob) {
        console.log('Updating table with SignalR job:', updatedJob);
        this.polling.updateJob(updatedJob);
      }
    }, { allowSignalWrites: true });
  }

  ngOnInit() {
    this.polling.loadReports();

    this.api.markNotificationsRead()
      .subscribe(() => {
        this.signalr.notificationCount.set(0);
      });
  }

  totalReports() {
    return this.polling.jobs().length;
  }

  completedReports() {
    return this.polling.jobs().filter(x => x.status === 'Completed').length;
  }

  inProgressReports() {
    return this.polling.jobs().filter(x => x.status === 'InProgress').length;
  }

  failedReports() {
    return this.polling.jobs().filter(x => x.status === 'Failed').length;
  }

}