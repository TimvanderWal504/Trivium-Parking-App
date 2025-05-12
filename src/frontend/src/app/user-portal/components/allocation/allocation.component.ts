// allocations/allocations.component.ts
import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { BehaviorSubject, merge, of } from 'rxjs';
import {
  catchError,
  map,
  startWith,
  switchMap,
  tap,
  filter,
  delay,
} from 'rxjs/operators';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { RefreshService } from '../../../core/services/refresh.service';
import { AllocationResponseDto } from '../../../models/allocation.dtos';

@Component({
  selector: 'app-allocations',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './allocation.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class AllocationComponent {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private refreshService = inject(RefreshService);

  private errorSub = new BehaviorSubject<string | null>(null);
  public error$ = this.errorSub.asObservable();

  private trigger$ = merge(
    this.authService.appUser$.pipe(filter((u) => !!u)),
    this.refreshService.reload$
  );

  private data$ = this.trigger$.pipe(
    tap(() => this.errorSub.next(null)),
    switchMap(() =>
      this.apiService.get<AllocationResponseDto>('allocations').pipe(
        delay(500),
        map((dto) => {
          return { alloc: dto ?? null, loading: false };
        }),
        catchError((err) => {
          console.error('Error fetching allocation', err);
          this.errorSub.next('Kon allocatie niet laden.');
          return of({ alloc: null, loading: false });
        }),
        startWith({
          alloc: null as AllocationResponseDto | null,
          loading: true,
        })
      )
    )
  );

  public allocation$ = this.data$.pipe(map((v) => v.alloc));
  public isLoading$ = this.data$.pipe(map((v) => v.loading));
}
