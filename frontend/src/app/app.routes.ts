import { Routes } from '@angular/router';
import { GenerateReportComponent } from './pages/generate-report/generate-report.component';
import { ReportsComponent } from './pages/reports/reports.component';

export const routes: Routes = [
  {
    path: '',
    redirectTo: 'generate',
    pathMatch: 'full'
  },
  {
    path: 'generate',
    component: GenerateReportComponent
  },
  {
    path: 'reports',
    component: ReportsComponent
  }
];