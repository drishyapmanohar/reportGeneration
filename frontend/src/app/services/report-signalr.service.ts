import { Injectable, NgZone, signal } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({
  providedIn: 'root'
})
export class ReportSignalrService {
  private hubConnection?: signalR.HubConnection;

  reportUpdated = signal<any | null>(null);
  notificationCount = signal(0);
  constructor(private zone: NgZone) {}

  startConnection() {
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
      });
    });

    this.hubConnection.on('ReportStatusUpdated', job => {

      if (job.status === 'Completed') {
        this.notificationCount.update(x => x + 1);
      }

      this.reportUpdated.set(job);
    });
  }
}