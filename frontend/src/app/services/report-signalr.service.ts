import { Injectable, NgZone, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ReportSignalrService {
  private hubConnection?: signalR.HubConnection;

  reportUpdated = signal<any | null>(null);
  notificationCount = signal(0);

  private completedJobIds = new Set<string>();

  constructor(private zone: NgZone) {}

  startConnection() {
    if (this.hubConnection) {
      return;
    }

    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5047/reportHub')
      .withAutomaticReconnect()
      .build();

    this.hubConnection
      .start()
      .then(() => console.log('SignalR connected'))
      .catch(err => console.error('SignalR error', err));

    this.hubConnection.on('ReportStatusUpdated', (job) => {
      console.log('SignalR job update received:', job);

      this.zone.run(() => {
        this.reportUpdated.set({ ...job });

        if (job.status === 'Completed' && !this.completedJobIds.has(job.id)) {
          this.completedJobIds.add(job.id);
          this.notificationCount.update(count => count + 1);
        }
      });
    });

    document.addEventListener('visibilitychange', () => {
      if (!document.hidden) {
        console.log('Tab active again');
      }
    });
  }
}