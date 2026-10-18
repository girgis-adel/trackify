export const apiBase = '/api/v1';

export interface ProblemDetails {
  title?: string;
  detail?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

export function apiErrorMessage(error: unknown): string {
  const response = error as { status?: number; error?: ProblemDetails };
  const problem = response?.error;
  if (problem?.errors) {
    const messages = Object.values(problem.errors).flat();
    if (messages.length) return messages.join(' ');
  }
  if (problem?.detail) return problem.detail;
  if (
    response?.status === 0 ||
    response?.status === 502 ||
    response?.status === 503 ||
    response?.status === 504
  )
    return 'Cannot reach the API. Check that the backend is running.';
  if (response?.status === 401) return 'Your session has expired. Please sign in again.';
  if (response?.status === 403) return 'Your account is not permitted to perform this action.';
  if (response?.status === 404) return 'The requested record could not be found.';
  if (response?.status === 409)
    return problem?.title || 'This conflicts with an existing record. Refresh and try again.';
  if (response?.status === 429) return 'Too many requests. Please try again shortly.';
  return problem?.title || 'Something went wrong. Please try again.';
}

export function fieldErrors(error: unknown): Record<string, string[]> {
  return (error as { error?: ProblemDetails })?.error?.errors ?? {};
}
