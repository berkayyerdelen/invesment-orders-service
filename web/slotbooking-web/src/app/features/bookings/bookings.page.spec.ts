import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { apiErrorInterceptor } from '../../core/errors/api-error.interceptor';
import { NotificationService } from '../../core/notifications/notification.service';
import { Booking, Room } from '../../core/api/models';
import { BookingsPage } from './bookings.page';

const rooms: Room[] = [{ id: 'room-a', name: 'Room A' }];

const booking: Booking = {
  id: 'b-1',
  roomId: 'room-a',
  roomName: 'Room A',
  startsAt: '2030-06-01T10:00:00Z',
  endsAt: '2030-06-01T11:00:00Z',
  guestName: 'Alice',
  status: 'Active',
  version: 3,
};

describe('BookingsPage', () => {
  let fixture: ComponentFixture<BookingsPage>;
  let backend: HttpTestingController;
  let notifications: NotificationService;

  beforeEach(() => {
    vi.spyOn(console, 'info').mockImplementation(() => {});
    vi.spyOn(console, 'warn').mockImplementation(() => {});
    vi.spyOn(console, 'error').mockImplementation(() => {});

    TestBed.configureTestingModule({
      imports: [BookingsPage],
      providers: [provideHttpClient(withInterceptors([apiErrorInterceptor])), provideHttpClientTesting()],
    });

    backend = TestBed.inject(HttpTestingController);
    notifications = TestBed.inject(NotificationService);
    fixture = TestBed.createComponent(BookingsPage);
    fixture.detectChanges();

    backend.expectOne('/api/rooms').flush(rooms);
    flushBookings([booking]);
    fixture.detectChanges();
  });

  afterEach(() => {
    backend.verify();
    vi.restoreAllMocks();
  });

  function flushBookings(bookings: Booking[]) {
    backend
      .match((request) => request.url === '/api/rooms/room-a/bookings')
      .forEach((request) => request.flush(bookings));
  }

  function text(): string {
    return (fixture.nativeElement as HTMLElement).textContent ?? '';
  }

  it('lists the bookings for the selected room', () => {
    expect(text()).toContain('Alice');
    expect(text()).toContain('Active');
  });

  it('blocks submission when the end is before the start', () => {
    const form = fixture.componentInstance.createForm;
    form.patchValue({ roomId: 'room-a', startsAt: '2030-06-01T12:00', endsAt: '2030-06-01T11:00', guestName: 'Bob' });
    fixture.detectChanges();

    const submit = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>('[data-testid="create-submit"]');
    expect(form.errors?.['time']).toMatch(/after the start/);
    expect(submit?.disabled).toBe(true);
  });

  it('shows a warning and reloads the list when a slot is already booked', () => {
    const page = fixture.componentInstance;
    const showSpy = vi.spyOn(notifications, 'show');

    page.createForm.patchValue({ roomId: 'room-a', startsAt: '2030-06-01T10:30', endsAt: '2030-06-01T11:30', guestName: 'Bob' });
    page.create();

    backend.expectOne('/api/bookings').flush(
      { title: 'Slot already booked', detail: 'The room is already booked for part of this time range.', status: 409 },
      { status: 409, statusText: 'Conflict' },
    );

    expect(showSpy).toHaveBeenCalledWith('warning', 'The room is already booked for part of this time range.');
    flushBookings([booking]);
    expect(page.bookings()).toEqual([booking]);
  });

  it('sends the version it read when saving an edit', () => {
    const page = fixture.componentInstance;
    page.startEdit(booking);
    page.editForm.patchValue({ guestName: 'Alice Updated' });

    page.saveEdit(booking);

    const request = backend.expectOne(`/api/bookings/${booking.id}`);
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toMatchObject({ version: 3, guestName: 'Alice Updated' });
    request.flush({ ...booking, guestName: 'Alice Updated', version: 4 });
    flushBookings([{ ...booking, guestName: 'Alice Updated', version: 4 }]);
  });
});
