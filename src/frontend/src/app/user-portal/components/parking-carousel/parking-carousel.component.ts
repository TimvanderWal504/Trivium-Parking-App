import {
  Component,
  inject,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  ElementRef,
  viewChild,
  viewChildren,
} from '@angular/core';
import { CommonModule, DatePipe } from '@angular/common';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { ParkingDay } from '../../../models/parking-day.model';
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
  take,
  withLatestFrom,
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
  private cdr = inject(ChangeDetectorRef);

  private errorMessageSubject = new BehaviorSubject<string | null>(null);
  public readonly errorMessage$ = this.errorMessageSubject.asObservable();

  private reload$ = new Subject<void>();
  private loadTrigger$ = merge(
    this.authService.appUser$.pipe(filter((u) => !!u)),
    this.reload$
  );

  readonly swiperEl = viewChild.required('swiperEl', { read: ElementRef });

  readonly slides = viewChildren('slide', { read: ElementRef });

  public today: Date;
  public tomorrow: Date;

  constructor() {
    this.today = new Date();
    this.today.setHours(0, 0, 0, 0);

    this.tomorrow = new Date(this.today);
    this.tomorrow.setDate(this.today.getDate() + 1);
  }

  private data$ = this.loadTrigger$.pipe(
    tap(() => this.errorMessageSubject.next(null)),
    switchMap(() => {
      const workDays = this.getNextWorkDays();
      return this.apiService
        .get<{ id: number; requestedDate: string }[]>('requests')
        .pipe(
          map((existingRequests) =>
            workDays.map((date) => {
              const existing = existingRequests.find((req) => {
                return this.isSameDate(new Date(req.requestedDate), date);
              });

              return {
                date,
                isRequested: !!existing,
                requestId: existing?.id ?? null,
                isLoading: false,
              } as ParkingDay;
            })
          ),
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

  public readonly parkingDays$ = this.data$.pipe(map((v) => v.days));
  public readonly isLoading$ = this.data$.pipe(map((v) => v.loading));

  public readonly slidesPerView$: Observable<number> = fromEvent(
    window,
    'resize'
  ).pipe(
    startWith(window.innerWidth),
    map(() => window.innerWidth),
    map((width) =>
      width >= 1280
        ? 5
        : width >= 1024
        ? 4
        : width >= 768
        ? 3
        : width >= 568
        ? 2
        : 1
    ),
    distinctUntilChanged()
  );

  ngAfterViewInit() {
    this.parkingDays$
      .pipe(
        filter((days) => days.length > 0),
        take(1)
      )
      .subscribe((days) => {
        const idx = days.findIndex((d) => !d.isRequested);
        const target = idx > -1 ? idx : 0;
        setTimeout(() => {
          const native = this.swiperEl().nativeElement as any;
          if (native?.swiper && target < 5) {
            native.swiper.slideTo(target, 0);
          }
        }, 20);
      });
  }

  toggleRequest(day: ParkingDay, index: number): void {
    this.errorMessageSubject.next(null);
    day.isLoading = true;

    let call$: Observable<{ Id: number } | null>;
    if (day.isRequested && day.requestId) {
      call$ = this.apiService
        .delete<void>(`requests/${day.requestId}`)
        .pipe(map(() => null));
    } else {
      call$ = this.apiService.post<{ Id: number }>('requests', {
        requestedDate: this.formatDateForApi(day.date),
      });
    }

    call$
      .pipe(
        withLatestFrom(this.parkingDays$),
        tap(([res, days]) => {
          if (day.isRequested) {
            day.isRequested = false;
            day.requestId = null;
          } else if (res) {
            day.isRequested = true;
            day.requestId = res.Id;

            const nextIdx = days.findIndex((d) => !d.isRequested);
            const native = this.swiperEl().nativeElement as any;
            if (native?.swiper && nextIdx < 5) {
              native.swiper.slideTo(nextIdx, 300);
            }
          }
          this.cdr.markForCheck();
        }),
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
        })
      )
      .subscribe();
  }

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

  public isSameDate(a: Date, b: Date): boolean {
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
