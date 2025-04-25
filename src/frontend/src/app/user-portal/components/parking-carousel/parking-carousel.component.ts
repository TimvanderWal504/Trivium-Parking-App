import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ParkingDay } from '../../../models/parking-day.model';
import { ParkingRequestResponseDto } from '../../../models/parking-request.dtos';
import { register } from 'swiper/element/bundle';
import {
  BehaviorSubject,
  Subject,
  merge,
  of,
  Observable,
  fromEvent,
} from 'rxjs';
import {
  catchError,
  finalize,
  map,
  shareReplay,
  startWith,
  switchMap,
  tap,
  filter,
  distinctUntilChanged,
} from 'rxjs/operators';

register();

@Component({
  selector: 'app-parking-carousel',
  standalone: true,
  imports: [CommonModule, DatePipe],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './parking-carousel.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingCarouselComponent {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);

  // Internal subject to hold error messages
  private errorMessageSubject = new BehaviorSubject<string | null>(null);
  public readonly errorMessage$ = this.errorMessageSubject.asObservable();

  // Trigger re-load after toggles
  private reload$ = new Subject<void>();

  // Combine login events and explicit reload triggers
  private loadTrigger$ = merge(
    this.authService.appUser$.pipe(filter((u) => !!u)),
    this.reload$
  );

  // Core stream: loads ParkingDay[] plus loading state
  private data$ = this.loadTrigger$.pipe(
    tap(() => this.errorMessageSubject.next(null)),
    switchMap(() => {
      const workDays = this.getNextWorkDays();
      return this.apiService.get<ParkingRequestResponseDto[]>('requests').pipe(
        map((existingRequests) => {
          return workDays.map((date) => {
            const existing = existingRequests.find((req) =>
              this.isSameDate(new Date(req.requestedDate), date)
            );
            return {
              date,
              isRequested: !!existing,
              requestId: existing?.id ?? null,
            } as ParkingDay;
          });
        }),
        map((days) => ({ days, loading: false })),
        catchError((err) => {
          console.error('Error fetching parking requests', err);
          this.errorMessageSubject.next('Could not load existing requests.');
          return of({ days: [] as ParkingDay[], loading: false });
        }),
        startWith({ days: [] as ParkingDay[], loading: true })
      );
    }),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  // Exposed streams for template binding
  public readonly parkingDays$ = this.data$.pipe(map((v) => v.days));
  public readonly isLoading$ = this.data$.pipe(map((v) => v.loading));

  // Compute slidesPerView dynamically based on window width
  public readonly slidesPerView$: Observable<number> = fromEvent(
    window,
    'resize'
  ).pipe(
    startWith(window.innerWidth),
    map(() => window.innerWidth),
    map((width) => {
      console.log(width);
      return width >= 1280
        ? 5
        : width >= 1024
        ? 4
        : width >= 768
        ? 3
        : width >= 568
        ? 2
        : 1;
    }),
    distinctUntilChanged()
  );

  // Toggle a parking request, then trigger reload()
  toggleRequest(day: ParkingDay): void {
    this.errorMessageSubject.next(null);
    day.isLoading = true;

    const apiCall$ =
      day.isRequested && day.requestId
        ? this.apiService.delete(`requests/${day.requestId}`)
        : this.apiService.post<ParkingRequestResponseDto>('requests', {
            requestedDate: this.formatDateForApi(day.date),
          });

    apiCall$
      .pipe(
        catchError((err) => {
          console.error(
            `Error ${day.isRequested ? 'revoking' : 'requesting'} parking`,
            err
          );
          this.errorMessageSubject.next(
            `Failed to ${
              day.isRequested ? 'revoke' : 'request'
            } parking. Please try again.`
          );
          return of(null);
        }),
        finalize(() => {
          day.isLoading = false;
          this.reload$.next();
        })
      )
      .subscribe();
  }

  // --- Helper methods ---

  private getNextWorkDays(): Date[] {
    const dates: Date[] = [];
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    let d = new Date(today);

    while (dates.length < 5) {
      const dow = d.getDay();
      if (dow >= 1 && dow <= 5) {
        dates.push(new Date(d));
      }
      d.setDate(d.getDate() + 1);
    }

    return dates;
  }

  private isSameDate(a: Date, b: Date): boolean {
    return (
      a.getFullYear() === b.getFullYear() &&
      a.getMonth() === b.getMonth() &&
      a.getDate() === b.getDate()
    );
  }

  private formatDateForApi(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
