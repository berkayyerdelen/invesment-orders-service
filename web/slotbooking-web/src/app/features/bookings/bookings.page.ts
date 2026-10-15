import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize } from 'rxjs';

import { BookingApiService } from '../../core/api/booking-api.service';
import { Booking, Room } from '../../core/api/models';
import { ApiError } from '../../core/errors/api-error';
import { LoggerService } from '../../core/logging/logger.service';
import { NotificationService } from '../../core/notifications/notification.service';
import { bookingTimesValidator } from './booking-times.rules';

export const RANGE_DAYS = 14;
const GUEST_NAME_MAX = 150;

@Component({
  selector: 'app-bookings-page',
  imports: [DatePipe, ReactiveFormsModule],
  templateUrl: './bookings.page.html',
  styleUrl: './bookings.page.scss',
})
export class BookingsPage implements OnInit {
  private readonly api = inject(BookingApiService);
  private readonly notifications = inject(NotificationService);
  private readonly log = inject(LoggerService).scope('BookingsPage');

  readonly rangeDays = RANGE_DAYS;
  readonly rooms = signal<Room[]>([]);
  readonly bookings = signal<Booking[]>([]);
  readonly selectedRoomId = signal<string | null>(null);
  readonly loadingRooms = signal(false);
  readonly loadingBookings = signal(false);
  readonly submitting = signal(false);
  readonly editingId = signal<string | null>(null);

  readonly createForm = new FormGroup(
    {
      roomId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
      startsAt: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
      endsAt: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
      guestName: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(GUEST_NAME_MAX)],
      }),
    },
    { validators: bookingTimesValidator(true) },
  );

  readonly editForm = new FormGroup(
    {
      startsAt: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
      endsAt: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
      guestName: new FormControl('', {
        nonNullable: true,
        validators: [Validators.required, Validators.maxLength(GUEST_NAME_MAX)],
      }),
    },
    { validators: bookingTimesValidator(false) },
  );

  ngOnInit(): void {
    this.loadingRooms.set(true);
    this.api
      .getRooms()
      .pipe(finalize(() => this.loadingRooms.set(false)))
      .subscribe({
        next: (rooms) => {
          this.rooms.set(rooms);
          if (rooms.length > 0) {
            this.selectRoom(rooms[0].id);
          }
        },
        error: (error: ApiError) => this.reportError('Could not load rooms', error),
      });
  }

  selectRoom(roomId: string): void {
    this.selectedRoomId.set(roomId);
    this.createForm.controls.roomId.setValue(roomId);
    this.editingId.set(null);
    this.loadBookings();
  }

  loadBookings(): void {
    const roomId = this.selectedRoomId();
    if (!roomId) {
      return;
    }

    const from = new Date();
    from.setHours(0, 0, 0, 0);
    const to = new Date(from);
    to.setDate(to.getDate() + RANGE_DAYS);

    this.loadingBookings.set(true);
    this.api
      .getBookings(roomId, from.toISOString(), to.toISOString())
      .pipe(finalize(() => this.loadingBookings.set(false)))
      .subscribe({
        next: (bookings) => this.bookings.set(bookings),
        error: (error: ApiError) => this.reportError('Could not load bookings', error),
      });
  }

  create(): void {
    if (this.createForm.invalid) {
      this.createForm.markAllAsTouched();
      return;
    }

    const value = this.createForm.getRawValue();
    this.submitting.set(true);
    this.api
      .create({
        roomId: value.roomId,
        startsAt: toIso(value.startsAt),
        endsAt: toIso(value.endsAt),
        guestName: value.guestName.trim(),
      })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (booking) => {
          this.log.info('Booking created', { bookingId: booking.id, version: booking.version });
          this.notifications.show('success', `Booked for ${booking.guestName}.`);
          this.createForm.patchValue({ startsAt: '', endsAt: '', guestName: '' });
          this.createForm.markAsPristine();
          this.loadBookings();
        },
        error: (error: ApiError) => this.handleWriteError(error),
      });
  }

  startEdit(booking: Booking): void {
    this.editingId.set(booking.id);
    this.editForm.setValue({
      startsAt: toLocalInput(booking.startsAt),
      endsAt: toLocalInput(booking.endsAt),
      guestName: booking.guestName,
    });
  }

  cancelEdit(): void {
    this.editingId.set(null);
  }

  saveEdit(booking: Booking): void {
    if (this.editForm.invalid) {
      this.editForm.markAllAsTouched();
      return;
    }

    const value = this.editForm.getRawValue();
    this.submitting.set(true);
    this.api
      .update(booking.id, {
        version: booking.version,
        startsAt: toIso(value.startsAt),
        endsAt: toIso(value.endsAt),
        guestName: value.guestName.trim(),
      })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          this.editingId.set(null);
          this.notifications.show('success', 'Booking updated.');
          this.loadBookings();
        },
        error: (error: ApiError) => this.handleWriteError(error),
      });
  }

  cancel(booking: Booking): void {
    this.submitting.set(true);
    this.api
      .cancel(booking.id, booking.version)
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: () => {
          this.notifications.show('success', 'Booking cancelled.');
          this.loadBookings();
        },
        error: (error: ApiError) => this.handleWriteError(error),
      });
  }

  /**
   * Conflicts (409) are expected: a slot was taken, or someone else changed the booking.
   * Tell the user, then reload so they see the current state and can retry.
   */
  private handleWriteError(error: ApiError): void {
    if (error.isConflict) {
      this.log.warn('Write conflict, reloading bookings', { traceId: error.traceId });
      this.notifications.show('warning', error.message);
      this.editingId.set(null);
      this.loadBookings();
      return;
    }
    this.reportError('The change could not be saved', error);
  }

  private reportError(message: string, error: ApiError): void {
    this.log.error(message, { status: error.status, traceId: error.traceId });
    this.notifications.show('error', `${message}: ${error.message}`);
  }
}

/** datetime-local value (local time, no zone) -> ISO 8601 UTC string. */
export function toIso(localValue: string): string {
  return new Date(localValue).toISOString();
}

/** ISO string -> datetime-local value in the user's local time zone. */
export function toLocalInput(iso: string): string {
  const date = new Date(iso);
  const offsetMs = date.getTimezoneOffset() * 60_000;
  return new Date(date.getTime() - offsetMs).toISOString().slice(0, 16);
}
