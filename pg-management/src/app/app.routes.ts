import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'dashboard'
  },
  {
    path: 'dashboard',
    loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
  },
  {
    path: 'rooms',
    loadChildren: () => import('./features/rooms/rooms.routes').then(m => m.ROOMS_ROUTES)
  },
  {
    path: 'tenants',
    loadChildren: () => import('./features/tenants/tenants.routes').then(m => m.TENANTS_ROUTES)
  },
  {
    path: 'bills',
    loadChildren: () => import('./features/bills/bills.routes').then(m => m.BILLS_ROUTES)
  },
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
