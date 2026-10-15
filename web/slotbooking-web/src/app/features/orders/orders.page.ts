import { DecimalPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { finalize, forkJoin } from 'rxjs';

import { OrderApiService } from '../../core/api/order-api.service';
import { Account, Order, OrderValidation, StockQuote } from '../../core/api/models';
import { ApiError } from '../../core/errors/api-error';
import { LoggerService } from '../../core/logging/logger.service';
import { NotificationService } from '../../core/notifications/notification.service';

export const STOCK_SYMBOL = 'AAPL';

@Component({
  selector: 'app-orders-page',
  imports: [DecimalPipe, ReactiveFormsModule],
  templateUrl: './orders.page.html',
  styleUrl: './orders.page.scss',
})
export class OrdersPage implements OnInit {
  private readonly api = inject(OrderApiService);
  private readonly notifications = inject(NotificationService);
  private readonly log = inject(LoggerService).scope('OrdersPage');

  readonly account = signal<Account | null>(null);
  readonly quote = signal<StockQuote | null>(null);
  readonly loading = signal(false);
  readonly reviewing = signal(false);
  readonly submitting = signal(false);
  readonly validation = signal<OrderValidation | null>(null);
  readonly executedOrder = signal<Order | null>(null);

  readonly form = new FormGroup({
    quantity: new FormControl<number | null>(null, { validators: [Validators.required, Validators.min(1)] }),
  });

  ngOnInit(): void {
    this.loading.set(true);
    forkJoin([this.api.getAccount(), this.api.getQuote(STOCK_SYMBOL)])
      .pipe(finalize(() => this.loading.set(false)))
      .subscribe({
        next: ([account, quote]) => {
          this.account.set(account);
          this.quote.set(quote);
        },
        error: (error: ApiError) => this.reportError('Could not load account or quote', error),
      });
  }

  review(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const account = this.account();
    const quote = this.quote();
    if (!account || !quote) {
      return;
    }

    this.reviewing.set(true);
    this.api
      .validate({ accountId: account.accountId, symbol: quote.symbol, quantity: this.form.getRawValue().quantity! })
      .pipe(finalize(() => this.reviewing.set(false)))
      .subscribe({
        next: (validation) => this.validation.set(validation),
        error: (error: ApiError) => this.reportError('Could not validate the order', error),
      });
  }

  editQuantity(): void {
    this.validation.set(null);
  }

  confirm(): void {
    const account = this.account();
    const quote = this.quote();
    if (!account || !quote) {
      return;
    }

    this.submitting.set(true);
    this.api
      .placeOrder({ accountId: account.accountId, symbol: quote.symbol, quantity: this.form.getRawValue().quantity! })
      .pipe(finalize(() => this.submitting.set(false)))
      .subscribe({
        next: (order) => {
          this.log.info('Order executed', { orderId: order.orderId, quantity: order.quantity });
          this.notifications.show('success', `Bought ${order.quantity} ${order.symbol} for $${order.totalAmount.toFixed(2)}.`);
          this.executedOrder.set(order);
          this.account.set({ accountId: account.accountId, cashBalance: order.remainingCashBalance });
          this.validation.set(null);
        },
        error: (error: ApiError) => this.reportError('Could not place the order', error),
      });
  }

  startNewOrder(): void {
    this.executedOrder.set(null);
    this.validation.set(null);
    this.form.reset();
  }

  private reportError(message: string, error: ApiError): void {
    this.log.error(message, { status: error.status, traceId: error.traceId });
    this.notifications.show('error', `${message}: ${error.message}`);
  }
}
