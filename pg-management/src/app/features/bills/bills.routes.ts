import { Routes } from '@angular/router';

export const BILLS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./bill-list/bill-list.component').then(m => m.BillListComponent)
  },
  {
    path: 'pending-dues',
    loadComponent: () => import('./pending-dues/pending-dues.component').then(m => m.PendingDuesComponent)
  },
  {
    path: 'generate',
    loadComponent: () => import('./generate-bill/generate-bill.component').then(m => m.GenerateBillComponent)
  }
];
