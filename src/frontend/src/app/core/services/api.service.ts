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
        this.http.get<T>(`${this.apiUrl}/${endpoint}`, {
          headers,
          params,
        })
      )
    );
  }

  post<T>(endpoint: string, body: any): Observable<T> {
    return this.buildHeaders$().pipe(
      switchMap((headers) =>
        this.http.post<T>(`${this.apiUrl}/${endpoint}`, body, {
          headers,
        })
      )
    );
  }

  put<T>(endpoint: string, body: any): Observable<T> {
    return this.buildHeaders$().pipe(
      switchMap((headers) =>
        this.http.put<T>(`${this.apiUrl}/${endpoint}`, body, {
          headers,
        })
      )
    );
  }

  delete<T>(endpoint: string): Observable<T> {
    return this.buildHeaders$().pipe(
      switchMap((headers) =>
        this.http.delete<T>(`${this.apiUrl}/${endpoint}`, { headers })
      )
    );
  }
}
