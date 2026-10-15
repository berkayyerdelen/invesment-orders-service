import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { NotificationService } from './notification.service';

describe('NotificationService', () => {
  let service: NotificationService;

  beforeEach(() => {
    vi.useFakeTimers();
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it('adds toasts in order with unique ids', () => {
    service.show('success', 'first');
    service.show('error', 'second');

    const toasts = service.toasts();
    expect(toasts.map((toast) => toast.message)).toEqual(['first', 'second']);
    expect(new Set(toasts.map((toast) => toast.id)).size).toBe(2);
  });

  it('dismisses a toast manually', () => {
    service.show('info', 'hello');
    const [toast] = service.toasts();

    service.dismiss(toast.id);

    expect(service.toasts()).toEqual([]);
  });

  it('removes a toast automatically after the timeout', () => {
    service.show('warning', 'slow down', 1000);

    vi.advanceTimersByTime(999);
    expect(service.toasts()).toHaveLength(1);

    vi.advanceTimersByTime(1);
    expect(service.toasts()).toHaveLength(0);
  });
});
