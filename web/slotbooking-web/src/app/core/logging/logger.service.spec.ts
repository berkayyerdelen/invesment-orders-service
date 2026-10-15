import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { LoggerService } from './logger.service';

describe('LoggerService', () => {
  let logger: LoggerService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    logger = TestBed.inject(LoggerService);
    vi.spyOn(console, 'info').mockImplementation(() => {});
    vi.spyOn(console, 'warn').mockImplementation(() => {});
    vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => vi.restoreAllMocks());

  it('writes info entries with the scope in the message', () => {
    logger.scope('Orders').info('Order placed', { orderId: 42 });

    expect(console.info).toHaveBeenCalledWith(
      '[INFO] Orders: Order placed',
      expect.objectContaining({ level: 'info', scope: 'Orders', message: 'Order placed', orderId: 42 }),
    );
  });

  it('routes warnings and errors to the matching console methods', () => {
    const scoped = logger.scope('Test');
    scoped.warn('careful');
    scoped.error('broken');

    expect(console.warn).toHaveBeenCalledOnce();
    expect(console.error).toHaveBeenCalledOnce();
  });

  it('includes a timestamp on every entry', () => {
    logger.scope('Test').info('hello');

    const [, entry] = vi.mocked(console.info).mock.calls[0] as [string, { timestamp: string }];
    expect(new Date(entry.timestamp).toString()).not.toBe('Invalid Date');
  });
});
