import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./features/orders/orders.page').then((m) => m.OrdersPage),
  },
  { path: '**', redirectTo: '' },
];
