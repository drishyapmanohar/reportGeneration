import { Injectable, signal } from '@angular/core';
import { ReportJob } from '../models/report-job';
import { ReportApiService } from './report-api.service';

@Injectable({
  providedIn: 'root'
})
export class ReportPollingService {

  jobs = signal<ReportJob[]>([]);
  totalPages = signal(1);
  totalCount = signal(0);

  constructor(private api: ReportApiService) {}

  loadReports(page = 1, pageSize = 10) {
    this.api.getMyReports(page, pageSize).subscribe(res => {
      this.jobs.set(res.items ?? []);
      this.totalCount.set(res.totalCount ?? 0);
      this.totalPages.set(res.totalPages ?? 1);
    });
  }

  updateJob(updatedJob: ReportJob) {
  this.jobs.update(jobs => {
    const existingJob = jobs.find(j =>
      j.id.toLowerCase() === updatedJob.id.toLowerCase()
    );

    // Ignore duplicate same-status SignalR events
    if (existingJob && existingJob.status === updatedJob.status) {
      return jobs;
    }

    const exists = !!existingJob;

    if (!exists) {
      return [updatedJob, ...jobs];
    }

    return jobs.map(job =>
      job.id.toLowerCase() === updatedJob.id.toLowerCase()
        ? { ...job, ...updatedJob }
        : job
    );
  });
}
}