// src/app/parking-request-info/parking-request-info.component.ts
import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  Input,
  inject,
} from '@angular/core';
import {
  FormBuilder,
  FormArray,
  FormControl,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import { CommonModule } from '@angular/common';
import { BehaviorSubject, Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { ApiService } from '../../../../core/services/api.service';
import { CreateVisitorParkingRequestDto } from '../../../../models/parking-request.dtos';
import { Location } from '../../../../models/parking-lot-location.dtos';
import { LocationSelectorComponent } from '../location-selector/location-selector.component';
import { VisitorListComponent } from '../visitor-list/visitor-list.component';

@Component({
  selector: 'app-parking-request-info',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    LocationSelectorComponent,
    VisitorListComponent,
  ],
  templateUrl: './parking-request-info.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingRequestInfoComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(ApiService);

  @Input() requestedDate!: Date;

  private tabSubject = new BehaviorSubject<'location' | 'visitors'>('location');
  readonly tab$ = this.tabSubject.asObservable();

  readonly filterForm = this.fb.group({
    country: new FormControl<string | null>(null, Validators.required),
    city: new FormControl<string | null>(null, Validators.required),
  });

  readonly visitorsForm = this.fb.group({
    visitors: this.fb.array([]),
  });
  get visitorsArr(): FormArray {
    return this.visitorsForm.get('visitors') as FormArray;
  }

  locations$!: Observable<Location[]>;

  ngOnInit() {
    this.locations$ = this.api
      .get<Location[]>('parkinglot/location')
      .pipe(shareReplay(1));

    this.addVisitor();
  }

  toggleTab(tab: 'location' | 'visitors') {
    this.tabSubject.next(tab);
  }

  addVisitor() {
    this.visitorsArr.push(
      this.fb.group({
        name: ['', [Validators.required]],
        email: ['', [Validators.required, Validators.email]],
      })
    );
  }

  removeVisitor(index: number) {
    this.visitorsArr.removeAt(index);
  }
  save() {
    if (this.filterForm.invalid || this.visitorsArr.length === 0) {
      return;
    }

    const { country, city } = this.filterForm.value!;
    const dto: CreateVisitorParkingRequestDto = {
      requestedDate: this.formatDateForApi(this.requestedDate),
      countryIsoCode: country!,
      city: city!,
      visitors: this.visitorsArr.value.map((v: any) => ({
        name: v.name,
        email: v.email,
      })),
    };

    this.api
      .post<CreateVisitorParkingRequestDto>('requests/visitors', dto)
      .subscribe({
        next: () => {
          this.filterForm.reset();
          this.visitorsArr.clear();
          this.addVisitor();
          this.tabSubject.next('location');
        },
        error: (err) => {
          console.error('Kon parking request niet aanmaken:', err);
        },
      });
  }

  private formatDateForApi(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
