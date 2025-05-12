import {
  Component,
  inject,
  ChangeDetectionStrategy,
  ChangeDetectorRef,
  ElementRef,
  ViewChild,
  ViewChildren,
  OnInit,
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
  fromEvent,
  defer,
  iif,
  Observable,
} from 'rxjs';
import {
  switchMap,
  map,
  catchError,
  startWith,
  tap,
  filter,
  distinctUntilChanged,
  take,
} from 'rxjs/operators';
import { CreateParkingRequestDto } from '../../../models/parking-request.dtos';

register();

interface State {
  days: ParkingDay[];
  loading: boolean;
  error: string | null;
}

@Component({
  selector: 'app-parking-carousel',
  standalone: true,
  imports: [CommonModule, DatePipe],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './parking-carousel.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingCarouselComponent implements OnInit {
  private apiService = inject(ApiService);
  private authService = inject(AuthService);
  private cdr = inject(ChangeDetectorRef);

  private state$ = new BehaviorSubject<State>({
    days: [],
    loading: true,
    error: null,
  });

  public readonly parkingDays$ = this.state$.pipe(map((s) => s.days));
  public readonly isLoading$ = this.state$.pipe(map((s) => s.loading));
  public readonly errorMessage$ = this.state$.pipe(map((s) => s.error));

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

  private reload$ = new Subject<void>();
  private loadTrigger$ = merge(
    this.authService.appUser$.pipe(filter((u) => !!u)),
    this.reload$
  );

  @ViewChild('swiperEl', { read: ElementRef, static: true })
  readonly swiperEl!: ElementRef;
  @ViewChildren('slide', { read: ElementRef }) readonly slides!: any;

  public today = new Date();
  public tomorrow = new Date();

  ngOnInit(): void {
    this.today.setHours(0, 0, 0, 0);
    this.tomorrow = new Date(this.today);
    this.tomorrow.setDate(this.today.getDate() + 1);

    this.loadTrigger$
      .pipe(
        tap(() => this.nextState({ loading: true })),
        switchMap(() => {
          const workDays = this.getNextWorkDays();
          return this.apiService
            .get<{ id: number; requestedDate: string }[]>('requests')
            .pipe(
              map((existing) =>
                workDays.map((date) => {
                  const ex = existing.find((r) =>
                    this.isSameDate(new Date(r.requestedDate), date)
                  );
                  return {
                    date,
                    isRequested: !!ex,
                    requestId: ex?.id ?? null,
                    isLoading: false,
                  } as ParkingDay;
                })
              ),
              map((days) => ({ days, loading: false } as State)),
              catchError((err) => {
                console.error('Error fetching requests', err);
                return of({
                  days: [],
                  loading: false,
                  error: 'Could not load existing requests.',
                } as State);
              })
            );
        })
      )
      .subscribe((s) => this.state$.next(s));
  }

  toggleRequest(day: ParkingDay): void {
    const { days } = this.state$.value;
    const loadingDays = days.map((d) =>
      this.isSameDate(d.date, day.date) ? { ...d, isLoading: true } : d
    );
    this.state$.next({ days: loadingDays, loading: false, error: null });

    const call$ = defer(() =>
      iif(
        () => day.isRequested,
        this.apiService
          .delete<void>(`requests/${day.requestId}`)
          .pipe(map(() => null)),
        this.apiService.post<{ id: number }>('requests', {
          requestedDate: this.formatDateForApi(day.date),
          countryIsoCode: 'NL',
          city: 'Zwolle',
        } as CreateParkingRequestDto)
      )
    );

    call$
      .pipe(
        take(1),
        tap((res) => {
          const updatedDays = loadingDays.map((d) => {
            if (this.isSameDate(d.date, day.date)) {
              const nowRequested = !day.isRequested;
              return {
                ...d,
                isLoading: false,
                isRequested: nowRequested,
                requestId: nowRequested ? res?.id ?? null : null,
              };
            }
            return d;
          });
          this.nextState({ days: updatedDays, loading: false, error: null });
        }),
        catchError((err) => {
          console.error(err);
          const reset = loadingDays.map((d) =>
            this.isSameDate(d.date, day.date) ? { ...d, isLoading: false } : d
          );
          this.nextState({
            days: reset,
            loading: false,
            error: `Failed to ${
              day.isRequested ? 'revoke' : 'request'
            } parking. Please try again.`,
          });
          return of(null);
        })
      )
      .subscribe();
  }

  private nextState(patch: Partial<State>) {
    this.state$.next({ ...this.state$.value, ...patch });
  }

  private getNextWorkDays(): Date[] {
    const dates: Date[] = [];
    const base = new Date(this.today);
    let d = new Date(base);
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
