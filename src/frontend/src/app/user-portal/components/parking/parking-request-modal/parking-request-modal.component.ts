// src/app/parking-request-modal/parking-request-modal.component.ts
import {
  Component,
  OnInit,
  ChangeDetectionStrategy,
  inject,
} from '@angular/core';
import {
  FormBuilder,
  FormArray,
  FormControl,
  Validators,
  ReactiveFormsModule,
} from '@angular/forms';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
  MatDialogModule,
} from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { BehaviorSubject, Observable } from 'rxjs';
import { shareReplay } from 'rxjs/operators';
import { ModalComponent } from '../../../../core/models/modal/modal.component';
import { ApiService } from '../../../../core/services/api.service';
import { CreateParkingRequestDto } from '../../../../models/parking-request.dtos';
import { LocationSelectorComponent } from '../location-selector/location-selector.component';
import { VisitorListComponent } from '../visitor-list/visitor-list.component';
import { Location } from '../../../../models/parking-lot-location.dtos';

@Component({
  selector: 'app-parking-request-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    ModalComponent,
    LocationSelectorComponent,
    VisitorListComponent,
  ],
  templateUrl: './parking-request-modal.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ParkingRequestModalComponent implements OnInit {
  private fb = inject(FormBuilder);
  private api = inject(ApiService);
  private dialogRef = inject(MatDialogRef<ParkingRequestModalComponent>);
  private dialogData = inject(MAT_DIALOG_DATA) as { date: Date };

  private tabSubject = new BehaviorSubject<'location' | 'visitors'>('location');
  readonly tab$ = this.tabSubject.asObservable();

  readonly requestedDate = this.dialogData.date;

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
        name: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
      })
    );
  }

  removeVisitor(i: number) {
    this.visitorsArr.removeAt(i);
  }

  close(cancel = true) {
    if (cancel) {
      this.dialogRef.close(null);
    } else {
      this.dialogRef.close(null);
    }
  }

  save() {
    if (this.filterForm.invalid || this.visitorsArr.length === 0) {
      return;
    }
    const { country, city } = this.filterForm.value!;
    const dto: CreateParkingRequestDto = {
      requestedDate: this.formatDateForApi(this.requestedDate),
      countryIsoCode: country!,
      city: city!,
      visitors: this.visitorsArr.value.map((v: any) => ({
        name: v.name,
        email: v.email,
      })),
    };

    this.dialogRef.close(dto);
  }

  private formatDateForApi(date: Date): string {
    const y = date.getFullYear();
    const m = String(date.getMonth() + 1).padStart(2, '0');
    const d = String(date.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
