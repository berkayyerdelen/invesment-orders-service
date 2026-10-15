import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';

import { LoggerService } from '../logging/logger.service';
import { ApiError, ProblemDetails } from './api-error';

/**
 * Converts every HTTP error into an ApiError and logs it at a level that matches its severity.
 * Components decide how to present the error to the user.
 */
export const apiErrorInterceptor: HttpInterceptorFn = (req, next) => {
  const log = inject(LoggerService).scope('HttpClient');

  return next(req).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse)) {
        return throwError(() => error);
      }

      const problem = (error.error ?? {}) as ProblemDetails;
      const apiError = new ApiError(
        error.status,
        problem.title ?? 'Request failed',
        problem.detail ?? (error.status === 0 ? 'The server cannot be reached.' : error.message),
        problem.traceId,
        problem.currentVersion,
      );

      const context = { method: req.method, url: req.url, status: apiError.status, traceId: apiError.traceId };
      if (apiError.status === 0 || apiError.status >= 500) {
        log.error(apiError.title, context);
      } else if (apiError.isConflict) {
        log.warn('Conflict returned by the API', context);
      } else {
        log.info('Request rejected by the API', context);
      }

      return throwError(() => apiError);
    }),
  );
};
