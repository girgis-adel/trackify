import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from './auth.service';

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const apiRequest = request.url.startsWith('/api/v1/');
  const protectedRequest =
    apiRequest &&
    (request.url.endsWith('/users/me') || request.method !== 'GET') &&
    !request.url.endsWith('/identity/login');
  const token = protectedRequest ? auth.token() : null;
  const outgoing = token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;
  return next(outgoing).pipe(
    catchError((error) => {
      if (protectedRequest && error.status === 401) auth.signOut();
      return throwError(() => error);
    }),
  );
};
