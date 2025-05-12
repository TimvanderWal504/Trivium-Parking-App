import { Injectable } from '@angular/core';
import { Subject, Observable } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class RefreshService {
  private reloadSubject = new Subject<void>();
  public readonly reload$: Observable<void> = this.reloadSubject.asObservable();

  public requestReload(): void {
    this.reloadSubject.next();
  }
}
