import { Routes } from '@angular/router';

export const ROOMS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./room-list/room-list.component').then(m => m.RoomListComponent)
  },
  {
    path: 'available',
    loadComponent: () => import('./room-availability/room-availability.component').then(m => m.RoomAvailabilityComponent)
  },
  {
    path: 'new',
    loadComponent: () => import('./room-form/room-form.component').then(m => m.RoomFormComponent)
  },
  {
    path: ':id/edit',
    loadComponent: () => import('./room-form/room-form.component').then(m => m.RoomFormComponent)
  }
];
