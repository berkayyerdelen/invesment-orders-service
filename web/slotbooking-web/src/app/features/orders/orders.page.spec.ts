import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { beforeEach, afterEach, describe, expect, it, vi } from 'vitest';

import { apiErrorInterceptor } from '../../core/errors/api-error.interceptor';
import { NotificationService } from '../../core/notifications/notification.service';
import { Account, Order, OrderValidation, StockQuote } from '../../core/api/models';
import { OrdersPage } from './orders.page';

const account: Account = { accountId: 'acc-1', cashBalance: 10_000 };
const quote: StockQuote = { symbol: 'AAPL', name: 'Apple Inc.', price: 150 };

describe('OrdersPage', () => {
  let fixture: ComponentFixture<OrdersPage>;
  let backend: HttpTestingController;
  let notifications: NotificationService;

  beforeEach(() => {
    vi.spyOn(console, 'info').mockImplementation(() => {});
    vi.spyOn(console, 'warn').mockImplementation(() => {});
    vi.spyOn(console, 'error').mockImplementation(() => {});

    TestBed.configureTestingModule({
      imports: [OrdersPage],
      providers: [provideHttpClient(withInterceptors([apiErrorInterceptor])), provideHttpClientTesting()],
    });

    backend = TestBed.inject(HttpTestingController);
    notifications = TestBed.inject(NotificationService);
    fixture = TestBed.createComponent(OrdersPage);
    fixture.detectChanges();

    backend.expectOne('/api/accounts/default').flush(account);
    backend.expectOne('/api/stocks/AAPL').flush(quote);
    fixture.detectChanges();
  });

  afterEach(() => {
    backend.verify();
    vi.restoreAllMocks();
  });

  function text(): string {
    return (fixture.nativeElement as HTMLElement).textContent ?? '';
  }

  it('shows the quote and cash balance', () => {
    expect(text()).toContain('Apple Inc.');
    expect(text()).toContain('150.00');
    expect(text()).toContain('10,000.00');
  });

  it('reviews the order before placing it', () => {
    const page = fixture.componentInstance;
    page.form.setValue({ quantity: 10 });
    page.review();

    const request = backend.expectOne('/api/orders/validate');
    expect(request.request.body).toEqual({ accountId: 'acc-1', symbol: 'AAPL', quantity: 10 });

    const validation: OrderValidation = { isValid: true, pricePerShare: 150, totalAmount: 1500, errorMessage: null };
    request.flush(validation);
    fixture.detectChanges();

    expect(text()).toContain('1,500.00');
  });

  it('disables confirm and shows the reason when funds are insufficient', () => {
    const page = fixture.componentInstance;
    page.form.setValue({ quantity: 1000 });
    page.review();

    const validation: OrderValidation = {
      isValid: false,
      pricePerShare: 150,
      totalAmount: 150_000,
      errorMessage: 'Order total $150000.00 exceeds available cash balance $10000.00.',
    };
    backend.expectOne('/api/orders/validate').flush(validation);
    fixture.detectChanges();

    const confirmButton = (fixture.nativeElement as HTMLElement).querySelector<HTMLButtonElement>(
      '[data-testid="confirm-submit"]',
    );
    expect(confirmButton?.disabled).toBe(true);
    expect(text()).toContain('exceeds available cash balance');
  });

  it('places the order and shows the executed confirmation', () => {
    const showSpy = vi.spyOn(notifications, 'show');
    const page = fixture.componentInstance;
    page.form.setValue({ quantity: 10 });
    page.review();

    const validation: OrderValidation = { isValid: true, pricePerShare: 150, totalAmount: 1500, errorMessage: null };
    backend.expectOne('/api/orders/validate').flush(validation);
    fixture.detectChanges();

    page.confirm();
    const order: Order = {
      orderId: 'order-1',
      symbol: 'AAPL',
      quantity: 10,
      pricePerShare: 150,
      totalAmount: 1500,
      executedAt: '2026-01-01T00:00:00Z',
      remainingCashBalance: 8_500,
    };
    backend.expectOne('/api/orders').flush(order);
    fixture.detectChanges();

    expect(showSpy).toHaveBeenCalledWith('success', 'Bought 10 AAPL for $1500.00.');
    expect(text()).toContain('Order executed');
    expect(text()).toContain('8,500.00');
  });
});
