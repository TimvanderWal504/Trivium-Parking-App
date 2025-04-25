import { Component, OnInit, inject } from '@angular/core';
import { Router } from '@angular/router';
import {
  filter,
  map,
  skip,
  skipWhile,
  take,
  tap,
  withLatestFrom,
} from 'rxjs/operators';
import { ResourceStatus } from '@angular/core';
import { toObservable } from '@angular/core/rxjs-interop';
import { AppUser, AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-homeredirect',
  template: '',
})
export class HomeRedirectComponent implements OnInit {
  private auth = inject(AuthService);
  private router = inject(Router);

  ngOnInit() {
    console.log('test');
    this.auth.appUser$
      .pipe(
        filter((user): user is AppUser => !!user),
        take(1),
        tap(() => {
          console.log('portaltest');
          this.router.navigate(['/portal']);
        })
      )
      .subscribe();

    this.auth.appStatus$
      .pipe(
        filter((status) => status === ResourceStatus.Resolved),
        take(1),
        withLatestFrom(this.auth.appUser$, this.auth.isLoggedIn$),
        tap(([_, user, isLoggedIn]) => {
          console.log('authtest');
          if (!user && !isLoggedIn) {
            this.router.navigate(['/auth']);
          }
        })
      )
      .subscribe();
  }
}
