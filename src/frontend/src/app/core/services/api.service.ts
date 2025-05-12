import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { Auth, authState, User } from '@angular/fire/auth';
import { rxResource, toObservable, toSignal } from '@angular/core/rxjs-interop';
import { from, Observable, throwError } from 'rxjs';
import { map, switchMap, take } from 'rxjs/operators';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private http = inject(HttpClient);
  private auth = inject(Auth);
  private apiUrl = environment.backendApiUrl;

  private readonly firebaseUser = toSignal(authState(this.auth), {
    initialValue: null,
  });

  private readonly idTokenResource = rxResource<string, User | null>({
    request: this.firebaseUser,
    loader: ({ request: user }) => {
      if (!user) {
        return throwError(() => new Error('User not authenticated'));
      }
      // van Promise naar Observable met from(...)
      return from(user.getIdToken());
    },
  });

  private readonly idToken$ = toObservable(this.idTokenResource.value);

  private buildHeaders$(): Observable<HttpHeaders> {
    return this.idToken$.pipe(
      take(1),
      map((token) => new HttpHeaders().set('Authorization', `Bearer ${token}`))
    );
  }

  get<T>(endpoint: string, params?: HttpParams): Observable<T> {
    return this.buildHeaders$().pipe(
      switchMap((headers) =>
        this.http
          .get<any>(`${this.apiUrl}/${endpoint}`, { headers, params })
          .pipe(map((body) => this.toCamelCase(body) as T))
      )
    );
  }

  post<T>(endpoint: string, body: any): Observable<T> {
    return this.buildHeaders$().pipe(
      switchMap((headers) =>
        this.http
          .post<any>(`${this.apiUrl}/${endpoint}`, body, { headers })
          .pipe(map((res) => this.toCamelCase(res) as T))
      )
    );
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.buildHeaders$().pipe(
      switchMap((headers) =>
        this.http
          .put<T>(`${this.apiUrl}/${endpoint}`, body, {
            headers,
          })
          .pipe(map((res) => this.toCamelCase(res) as T))
      )
    );
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.buildHeaders$().pipe(
      switchMap((headers) =>
        this.http
          .delete<T>(`${this.apiUrl}/${endpoint}`, { headers })
          .pipe(map((res) => this.toCamelCase(res) as T))
      )
    );
  }

  private toCamelCase(input: any): any {
    if (Array.isArray(input)) {
      return input.map((v) => this.toCamelCase(v));
    } else if (input !== null && typeof input === 'object') {
      return Object.entries(input).reduce((acc, [key, value]) => {
        const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
        acc[camelKey] = this.toCamelCase(value);
        return acc;
      }, {} as Record<string, any>);
    }

    return input;
  }
}
