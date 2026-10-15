import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/bookings/bookings.page').then((m) => m.BookingsPage),
  },
  { path: '**', redirectTo: '' },
];
