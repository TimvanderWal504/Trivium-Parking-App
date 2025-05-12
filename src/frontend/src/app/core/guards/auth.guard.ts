import { CanActivateFn } from '@angular/router';
import { inject, ResourceStatus } from '@angular/core';
import { filter, take, map } from 'rxjs/operators';
import { combineLatest } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = () => {
  const auth = inject(AuthService);

  return combineLatest([auth.appStatus$, auth.appUser$]).pipe(
    filter(
      ([status, user]) =>
        status === ResourceStatus.Resolved &&
        user !== null &&
        user !== undefined
    ),
    take(1),
    map((state) => {
      return true;
    })
  );
};
