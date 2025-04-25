import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  FormGroup,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { ApiService } from '../../../core/services/api.service';
import { HttpErrorResponse } from '@angular/common/http';
import { Subject, Observable, of } from 'rxjs';
import {
  filter,
  switchMap,
  tap,
  map,
  catchError,
  startWith,
  shareReplay,
  take,
} from 'rxjs/operators';

@Component({
  selector: 'app-user-create',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './user-create.component.html',
})
export class UserCreateComponent implements OnInit {
  createUserForm!: FormGroup;

  // 1) Subject om submits te triggeren
  private submit$ = new Subject<void>();

  // 2) result$ emite bij elke submit { response, error }
  result$: Observable<{
    response: any | null;
    error: HttpErrorResponse | null;
  }> = this.submit$.pipe(
    // enkel verder als form valid is
    filter(() => this.createUserForm.valid),
    // haal de API call op
    switchMap(() => {
      const { email, password, displayName } = this.createUserForm.value;
      const payload = {
        email,
        password,
        displayName: displayName || null,
      };
      return this.apiService.post<any>('users', payload).pipe(
        map((response) => ({ response, error: null })),
        catchError((error) => of({ response: null, error }))
      );
    }),
    // shareReplay zodat late subscribers laatste status pakken
    shareReplay({ bufferSize: 1, refCount: true })
  );

  // 3) isLoading$ is true vanaf submit tot result$ één keer emit
  isLoading$: Observable<boolean> = this.submit$.pipe(
    switchMap(() =>
      this.result$.pipe(
        // eerst loading=true, dan false zodra er een result is
        map(() => false),
        startWith(true)
      )
    ),
    shareReplay({ bufferSize: 1, refCount: true })
  );

  // 4) successMessage$ en errorMessage$ mappen result$
  successMessage$: Observable<string | null> = this.result$.pipe(
    map(({ response, error }) =>
      response
        ? `User ${
            response.email || this.createUserForm.value.email
          } created successfully!`
        : null
    )
  );

  errorMessage$: Observable<string | null> = this.result$.pipe(
    map(({ response, error }) => {
      if (!error) return null;
      return typeof error.error === 'string'
        ? error.error
        : error.message || 'Er is iets misgegaan';
    })
  );

  // inject FormBuilder en ApiService
  private fb = inject(FormBuilder);
  private apiService = inject(ApiService);

  ngOnInit(): void {
    this.createUserForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      displayName: [''],
      password: ['', [Validators.required, Validators.minLength(6)]],
    });
  }

  onSubmit(): void {
    // forceer validatie weergave
    this.createUserForm.markAllAsTouched();
    if (this.createUserForm.valid) {
      // trigger de hele pipeline
      this.submit$.next();

      // optioneel: reset form ná succes
      this.result$
        .pipe(
          filter((r) => !!r.response),
          take(1),
          tap(() => this.createUserForm.reset())
        )
        .subscribe();
    }
  }
}
