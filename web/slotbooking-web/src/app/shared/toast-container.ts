import { Component, inject } from '@angular/core';

import { NotificationService } from '../core/notifications/notification.service';

@Component({
  selector: 'app-toast-container',
  template: `
    <div class="toasts" aria-live="polite">
      @for (toast of notifications.toasts(); track toast.id) {
        <div class="toast {{ toast.kind }}" role="status">
          <span>{{ toast.message }}</span>
          <button type="button" (click)="notifications.dismiss(toast.id)" aria-label="Dismiss">×</button>
        </div>
      }
    </div>
  `,
  styles: `
    .toasts { position: fixed; right: 1rem; bottom: 1rem; display: flex; flex-direction: column; gap: 0.5rem; z-index: 1000; max-width: 360px; }
    .toast { display: flex; justify-content: space-between; gap: 0.75rem; align-items: start; padding: 0.7rem 0.9rem; border-radius: 8px; color: #fff; box-shadow: 0 4px 12px rgba(0,0,0,.15); font-size: 0.9rem; }
    .success { background: #16a34a; }
    .info { background: #2563eb; }
    .warning { background: #d97706; }
    .error { background: #dc2626; }
    button { background: none; border: none; color: inherit; font-size: 1.1rem; cursor: pointer; }
  `,
})
export class ToastContainer {
  readonly notifications = inject(NotificationService);
}
