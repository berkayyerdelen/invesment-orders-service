import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { Account, Order, OrderRequest, OrderValidation, StockQuote } from './models';

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api';

  getAccount(): Observable<Account> {
    return this.http.get<Account>(`${this.baseUrl}/accounts/default`);
  }

  getQuote(symbol: string): Observable<StockQuote> {
    return this.http.get<StockQuote>(`${this.baseUrl}/stocks/${symbol}`);
  }

  validate(request: OrderRequest): Observable<OrderValidation> {
    return this.http.post<OrderValidation>(`${this.baseUrl}/orders/validate`, request);
  }

  placeOrder(request: OrderRequest): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/orders`, request);
  }
}
