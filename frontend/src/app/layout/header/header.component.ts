import { Component, OnInit, effect, signal } from '@angular/core';
import { NavigationEnd, Router, RouterLink } from '@angular/router';
import { ReportApiService } from '../../services/report-api.service';
import { ReportSignalrService } from '../../services/report-signalr.service';
import { filter } from 'rxjs';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './header.component.html',
  styleUrl: './header.component.scss'
})
export class HeaderComponent implements OnInit {
  notificationCount = this.signalr.notificationCount;

  constructor(
  private api: ReportApiService,
  private signalr: ReportSignalrService,
  private router: Router
) {
  effect(() => {
    const job = this.signalr.reportUpdated();

    if (job?.status === 'Completed') {
      this.loadNotificationCount();
    }
  });

  this.router.events
    .pipe(filter(event => event instanceof NavigationEnd))
    .subscribe(() => {
      this.loadNotificationCount();
    });
}
  ngOnInit() {
    this.loadNotificationCount();
  }

  loadNotificationCount() {
    this.api.getNotificationCount()
      .subscribe(count => this.signalr.notificationCount.set(count));
  }

  clearNotifications() {
    this.api.markNotificationsRead()
      .subscribe({
        next: () => {
          console.log('Notifications marked as read');
          this.notificationCount.set(0);
        },
        error: err => console.error(err)
      });
  }
}