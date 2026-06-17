import { Injectable, signal } from '@angular/core';
import { interval, startWith, switchMap, takeWhile } from 'rxjs';
import { ReportJob } from '../models/report-job';
import { ReportApiService } from './report-api.service';

@Injectable({
  providedIn: 'root'
})
export class ReportPollingService {
  jobs = signal<ReportJob[]>([]);
  private activeJobIds = new Set<string>();

  constructor(private api: ReportApiService) {}

  loadReports() {
    this.api.getMyReports().subscribe((jobs: ReportJob[]) => {
      this.jobs.set(jobs);
    });
  }

  startPolling(jobId: string) {
    if (this.activeJobIds.has(jobId)) {
      return;
    }

    this.activeJobIds.add(jobId);

    interval(5000).pipe(
      startWith(0),
      switchMap(() => this.api.getStatus(jobId)),
      takeWhile(
        job => job.status !== 'Completed' && job.status !== 'Failed',
        true
      )
    ).subscribe((job: ReportJob) => {
      this.updateJob(job);

      if (job.status === 'Completed' || job.status === 'Failed') {
        this.activeJobIds.delete(jobId);
      }
    });
  }

  private updateJob(job: ReportJob) {
    const currentJobs = this.jobs();
    const filteredJobs = currentJobs.filter(x => x.id !== job.id);
    this.jobs.set([job, ...filteredJobs]);
  }
}