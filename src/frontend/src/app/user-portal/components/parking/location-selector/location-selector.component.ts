import {
  Component,
  Input,
  OnInit,
  ChangeDetectionStrategy,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';
import { Observable, combineLatest } from 'rxjs';
import { startWith, map } from 'rxjs/operators';
import { Location } from '../../../../models/parking-lot-location.dtos';
@Component({
  selector: 'app-location-selector',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './location-selector.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class LocationSelectorComponent implements OnInit {
  @Input() filterForm!: FormGroup;
  @Input() locations$!: Observable<Location[]>;

  countries$!: Observable<{ code: string; name: string }[]>;
  cities$!: Observable<string[]>;

  ngOnInit() {
    const displayNames = new Intl.DisplayNames(['en'], { type: 'region' });

    this.countries$ = this.locations$.pipe(
      map((locs) =>
        Array.from(
          new Map(
            locs.map((l) => [
              l.countryIsoCode,
              displayNames.of(l.countryIsoCode) || l.countryIsoCode,
            ])
          )
        ).map(([code, name]) => ({ code, name }))
      )
    );

    this.cities$ = combineLatest([
      this.locations$,
      this.filterForm
        .get('country')!
        .valueChanges.pipe(startWith(this.filterForm.get('country')!.value)),
    ]).pipe(
      map(([locs, country]) =>
        Array.from(
          new Set(
            locs
              .filter((l) => !country || l.countryIsoCode === country)
              .map((l) => l.city)
          )
        )
      )
    );
  }
}
