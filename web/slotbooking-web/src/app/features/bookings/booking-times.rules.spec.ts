import { describe, expect, it } from 'vitest';

import { MAX_BOOKING_DURATION_MS, validateBookingTimes } from './booking-times.rules';

const now = new Date('2030-06-01T08:00:00Z');

describe('validateBookingTimes', () => {
  it('accepts a valid future range', () => {
    expect(validateBookingTimes('2030-06-01T10:00:00Z', '2030-06-01T11:00:00Z', now, true)).toBeNull();
  });

  it('rejects an end that is not after the start', () => {
    expect(validateBookingTimes('2030-06-01T10:00:00Z', '2030-06-01T10:00:00Z', now, true)).toMatch(/after the start/);
    expect(validateBookingTimes('2030-06-01T11:00:00Z', '2030-06-01T10:00:00Z', now, true)).toMatch(/after the start/);
  });

  it('rejects bookings longer than 24 hours', () => {
    const start = '2030-06-01T10:00:00Z';
    const end = new Date(new Date(start).getTime() + MAX_BOOKING_DURATION_MS + 60_000).toISOString();

    expect(validateBookingTimes(start, end, now, true)).toMatch(/24 hours/);
  });

  it('rejects a start in the past when a future start is required', () => {
    expect(validateBookingTimes('2030-06-01T07:00:00Z', '2030-06-01T09:00:00Z', now, true)).toMatch(/future/);
  });

  it('allows a past start when a future start is not required', () => {
    expect(validateBookingTimes('2030-06-01T07:00:00Z', '2030-06-01T09:00:00Z', now, false)).toBeNull();
  });

  it('ignores empty values so required-field errors take over', () => {
    expect(validateBookingTimes('', '2030-06-01T11:00:00Z', now, true)).toBeNull();
  });

  it('rejects unparseable dates', () => {
    expect(validateBookingTimes('not a date', '2030-06-01T11:00:00Z', now, true)).toMatch(/valid/);
  });
});
