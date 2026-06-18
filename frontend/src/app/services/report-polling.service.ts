import { Injectable, signal } from '@angular/core';
import { ReportJob } from '../models/report-job';
import { ReportApiService } from './report-api.service';

@Injectable({
  providedIn: 'root'
})
export class ReportPollingService {
  jobs = signal<ReportJob[]>([]);

  constructor(private api: ReportApiService) {}

  loadReports() {
    this.api.getMyReports().subscribe((jobs: ReportJob[]) => {
      this.jobs.set(jobs);
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