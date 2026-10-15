/** Normalized error for every failed API call, built from RFC 7807 ProblemDetails when present. */
export class ApiError extends Error {
  constructor(
    readonly status: number,
    readonly title: string,
    message: string,
    readonly traceId?: string,
    readonly currentVersion?: number,
  ) {
    super(message);
    this.name = 'ApiError';
  }

  get isConflict(): boolean {
    return this.status === 409;
  }

  get isNetworkError(): boolean {
    return this.status === 0;
  }
}

/** Shape of an RFC 7807 problem response as produced by the API. */
export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  currentVersion?: number;
}
