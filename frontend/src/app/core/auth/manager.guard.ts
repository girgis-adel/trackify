import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from './auth.service';

export const managerGuard: CanActivateFn = async (_route, state) => {
  const auth = inject(AuthService);
  const router = inject(Router);
  try {
    await auth.restore();
  } catch {
    return router.createUrlTree(['/unavailable']);
  }
  if (!auth.isAuthenticated())
    return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
  return auth.canManageCatalog() ? true : router.createUrlTree(['/forbidden']);
};
