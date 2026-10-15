import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it, vi } from 'vitest';

import { ApiError } from './api-error';
import { apiErrorInterceptor } from './api-error.interceptor';

describe('apiErrorInterceptor', () => {
  let http: HttpClient;
  let backend: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([apiErrorInterceptor])), provideHttpClientTesting()],
    });
    http = TestBed.inject(HttpClient);
    backend = TestBed.inject(HttpTestingController);
    vi.spyOn(console, 'info').mockImplementation(() => {});
    vi.spyOn(console, 'warn').mockImplementation(() => {});
    vi.spyOn(console, 'error').mockImplementation(() => {});
  });

  afterEach(() => {
    backend.verify();
    vi.restoreAllMocks();
  });

  it('turns a ProblemDetails conflict into an ApiError with the current version', async () => {
    const promise = new Promise<ApiError>((resolve, reject) =>
      http.put('/api/bookings/1', {}).subscribe({ error: (error) => resolve(error), next: () => reject() }),
    );

    backend.expectOne('/api/bookings/1').flush(
      { title: 'Conflict', detail: 'Changed by someone else.', status: 409, traceId: 'abc', currentVersion: 7 },
      { status: 409, statusText: 'Conflict' },
    );

    const error = await promise;
    expect(error).toBeInstanceOf(ApiError);
    expect(error.status).toBe(409);
    expect(error.isConflict).toBe(true);
    expect(error.message).toBe('Changed by someone else.');
    expect(error.traceId).toBe('abc');
    expect(error.currentVersion).toBe(7);
  });

  it('reports a network failure as status 0', async () => {
    const promise = new Promise<ApiError>((resolve) =>
      http.get('/api/rooms').subscribe({ error: (error) => resolve(error) }),
    );

    backend.expectOne('/api/rooms').error(new ProgressEvent('error'));

    const error = await promise;
    expect(error.isNetworkError).toBe(true);
    expect(error.message).toBe('The server cannot be reached.');
  });

  it('logs server errors at error level', async () => {
    const promise = new Promise<unknown>((resolve) =>
      http.get('/api/rooms').subscribe({ error: (error) => resolve(error) }),
    );

    backend.expectOne('/api/rooms').flush({ title: 'Unexpected error' }, { status: 500, statusText: 'Server Error' });
    await promise;

    expect(console.error).toHaveBeenCalled();
  });
});
