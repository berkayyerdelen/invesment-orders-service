import { Injectable, signal } from '@angular/core';

export type ToastKind = 'success' | 'warning' | 'error' | 'info';

export interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private nextId = 0;
  private readonly toastList = signal<Toast[]>([]);

  readonly toasts = this.toastList.asReadonly();

  show(kind: ToastKind, message: string, timeoutMs = 5000): void {
    const id = ++this.nextId;
    this.toastList.update((list) => [...list, { id, kind, message }]);
    setTimeout(() => this.dismiss(id), timeoutMs);
  }

  dismiss(id: number): void {
    this.toastList.update((list) => list.filter((toast) => toast.id !== id));
  }
}
