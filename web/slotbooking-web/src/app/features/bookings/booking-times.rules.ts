import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/** Mirrors the server rules in Booking.cs. The server stays authoritative; this gives instant feedback. */
export const MAX_BOOKING_DURATION_MS = 24 * 60 * 60 * 1000;
export const START_TOLERANCE_MS = 5 * 60 * 1000;

/** Returns a user-facing message for the first broken rule, or null when the times are valid. */
export function validateBookingTimes(
  startsAt: string,
  endsAt: string,
  now: Date,
  requireFutureStart: boolean,
): string | null {
  if (!startsAt || !endsAt) {
    return null; // required-field errors are shown by the individual controls
  }

  const start = new Date(startsAt);
  const end = new Date(endsAt);
  if (Number.isNaN(start.getTime()) || Number.isNaN(end.getTime())) {
    return 'Enter a valid start and end time.';
  }
  if (end.getTime() <= start.getTime()) {
    return 'The end time must be after the start time.';
  }
  if (end.getTime() - start.getTime() > MAX_BOOKING_DURATION_MS) {
    return 'A booking cannot be longer than 24 hours.';
  }
  if (requireFutureStart && start.getTime() < now.getTime() - START_TOLERANCE_MS) {
    return 'The booking must start in the future.';
  }
  return null;
}

/** Group validator for a form with startsAt / endsAt controls. Error key: `time`. */
export function bookingTimesValidator(requireFutureStart: boolean, now: () => Date = () => new Date()): ValidatorFn {
  return (group: AbstractControl): ValidationErrors | null => {
    const message = validateBookingTimes(
      group.get('startsAt')?.value ?? '',
      group.get('endsAt')?.value ?? '',
      now(),
      requireFutureStart,
    );
    return message ? { time: message } : null;
  };
}
