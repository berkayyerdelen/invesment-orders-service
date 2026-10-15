import { HttpInterceptorFn } from '@angular/common/http';

const CORRELATION_ID_HEADER = 'X-Correlation-Id';

// One id for the lifetime of this app load, attached to every API request so the Aspire dashboard
// (Structured Logs / Traces) can link everything in this browser session, independent of each
// request's own trace id (which resets per request).
const sessionCorrelationId = crypto.randomUUID();

export const correlationIdInterceptor: HttpInterceptorFn = (req, next) =>
  next(req.clone({ setHeaders: { [CORRELATION_ID_HEADER]: sessionCorrelationId } }));
