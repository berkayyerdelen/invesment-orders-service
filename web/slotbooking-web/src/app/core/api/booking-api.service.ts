import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { Booking, CreateBookingRequest, Room, UpdateBookingRequest } from './models';

@Injectable({ providedIn: 'root' })
export class BookingApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  getRooms(): Observable<Room[]> {
    return this.http.get<Room[]>(`${this.baseUrl}/rooms`);
  }

  getBookings(roomId: string, from: string, to: string): Observable<Booking[]> {
    return this.http.get<Booking[]>(`${this.baseUrl}/rooms/${roomId}/bookings`, { params: { from, to } });
  }

  create(request: CreateBookingRequest): Observable<Booking> {
    return this.http.post<Booking>(`${this.baseUrl}/bookings`, request);
  }

  update(id: string, request: UpdateBookingRequest): Observable<Booking> {
    return this.http.put<Booking>(`${this.baseUrl}/bookings/${id}`, request);
  }

  cancel(id: string, version: number): Observable<Booking> {
    return this.http.post<Booking>(`${this.baseUrl}/bookings/${id}/cancel`, { version });
  }
}
