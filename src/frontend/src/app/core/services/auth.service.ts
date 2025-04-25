import { Injectable, ResourceStatus, inject } from '@angular/core';
import { Auth, authState, User, signOut } from '@angular/fire/auth';
import { toSignal, rxResource, toObservable } from '@angular/core/rxjs-interop';
import {
  BehaviorSubject,
  catchError,
  combineLatestWith,
  distinctUntilChanged,
  filter,
  map,
  Observable,
  of,
  take,
  tap,
} from 'rxjs';
import { Router } from '@angular/router';
import { ApiService } from './api.service';

export interface AppUser {
  id: number;
  firebaseUid: string;
  email: string | null;
  displayName: string | null;
  roles: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private _isLoggedIn$ = new BehaviorSubject<boolean>(false);
  private auth = inject(Auth);
  private router = inject(Router);
  private apiService = inject(ApiService);

  private firebaseUser = toSignal(authState(this.auth), { initialValue: null });

  readonly isLoggedIn$: Observable<boolean> = this._isLoggedIn$.asObservable();

  constructor() {
    const token = localStorage.getItem('token');
    this._isLoggedIn$.next(!!token);
  }

  public readonly appUserResource = rxResource<AppUser | null, User | null>({
    request: this.firebaseUser,
    loader: ({ request: firebaseUser }) => {
      if (!firebaseUser) {
        return of(null);
      }

      const payload = {
        firebaseUid: firebaseUser.uid,
        email: firebaseUser.email,
        displayName: firebaseUser.displayName,
      };

      return this.apiService.post<AppUser>('auth/sync', payload).pipe(
        tap((appUser) => {
          localStorage.setItem('token', appUser.firebaseUid);
          this._isLoggedIn$.next(true);
        }),
        catchError((err) => {
          console.error('sync-fail:', err);
          return of(null);
        })
      );
    },
  });

  public readonly appUser$ = toObservable(this.appUserResource.value);
  public readonly appStatus$ = toObservable(this.appUserResource.status);

  async signOut(): Promise<void> {
    try {
      localStorage.removeItem('token');
      this._isLoggedIn$.next(false);
      await signOut(this.auth);
      this.router.navigate(['/auth']);
    } catch (error) {
      console.error('AuthService: Error signing out', error);
    }
  }
}
