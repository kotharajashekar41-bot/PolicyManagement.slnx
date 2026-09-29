import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./features/policies/policy-dashboard/policy-dashboard.component').then(
        (m) => m.PolicyDashboardComponent
      ),
    title: 'Policies — Chubb APAC'
  },
  { path: '**', redirectTo: '' }
];
