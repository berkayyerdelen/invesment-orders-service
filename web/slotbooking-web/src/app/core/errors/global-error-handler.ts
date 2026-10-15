import { ErrorHandler, Injectable, inject } from '@angular/core';

import { LoggerService } from '../logging/logger.service';
import { NotificationService } from '../notifications/notification.service';

/** Catches anything that escapes the component tree, logs it, and tells the user something went wrong. */
@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  private readonly log = inject(LoggerService).scope('GlobalErrorHandler');
  private readonly notifications = inject(NotificationService);

  handleError(error: unknown): void {
    const message = error instanceof Error ? error.message : String(error);
    this.log.error('Unhandled error', { message, stack: error instanceof Error ? error.stack : undefined });
    this.notifications.show('error', 'Something went wrong. Please try again.');
  }
}
