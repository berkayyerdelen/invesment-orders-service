import { Injectable, isDevMode } from '@angular/core';

export type LogLevel = 'debug' | 'info' | 'warn' | 'error';

const LEVEL_ORDER: Record<LogLevel, number> = { debug: 10, info: 20, warn: 30, error: 40 };

/**
 * Small structured logger. Debug output is only emitted in development builds.
 * Replace the console sink with a remote sink (e.g. an OpenTelemetry collector) when needed.
 */
@Injectable({ providedIn: 'root' })
export class LoggerService {
  private readonly minLevel: LogLevel = isDevMode() ? 'debug' : 'info';

  scope(name: string): ScopedLogger {
    return new ScopedLogger(this, name);
  }

  log(level: LogLevel, scope: string, message: string, context?: Record<string, unknown>): void {
    if (LEVEL_ORDER[level] < LEVEL_ORDER[this.minLevel]) {
      return;
    }

    const entry = { timestamp: new Date().toISOString(), level, scope, message, ...context };
    const sink = level === 'error' ? console.error : level === 'warn' ? console.warn : console.info;
    sink(`[${entry.level.toUpperCase()}] ${scope}: ${message}`, entry);
  }
}

export class ScopedLogger {
  constructor(private readonly logger: LoggerService, private readonly scopeName: string) {}

  debug(message: string, context?: Record<string, unknown>): void {
    this.logger.log('debug', this.scopeName, message, context);
  }

  info(message: string, context?: Record<string, unknown>): void {
    this.logger.log('info', this.scopeName, message, context);
  }

  warn(message: string, context?: Record<string, unknown>): void {
    this.logger.log('warn', this.scopeName, message, context);
  }

  error(message: string, context?: Record<string, unknown>): void {
    this.logger.log('error', this.scopeName, message, context);
  }
}
