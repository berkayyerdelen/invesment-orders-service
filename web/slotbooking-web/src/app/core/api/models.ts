export interface Room {
  id: string;
  name: string;
}

export type BookingStatus = 'Active' | 'Cancelled';

export interface Booking {
  id: string;
  roomId: string;
  roomName: string;
  startsAt: string;
  endsAt: string;
  guestName: string;
  status: BookingStatus;
  /** Concurrency token. Send it back with every update so the server can detect stale writes. */
  version: number;
}

export interface CreateBookingRequest {
  roomId: string;
  startsAt: string;
  endsAt: string;
  guestName: string;
}

export interface UpdateBookingRequest {
  version: number;
  startsAt: string;
  endsAt: string;
  guestName: string;
}
