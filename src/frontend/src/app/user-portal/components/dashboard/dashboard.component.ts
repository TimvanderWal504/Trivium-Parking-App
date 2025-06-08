import { AllocationComponent } from './../allocation/allocation.component';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ParkingCarouselComponent } from '../parking/parking-carousel/parking-carousel.component';
import { HeaderComponent } from '../../../core/components/header/header.component';
import { ParkingRequestInfoComponent } from '../parking/parking-request-info/parking-request-info.component';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    HeaderComponent,
    ParkingCarouselComponent,
    AllocationComponent,
    ParkingRequestInfoComponent,
  ],
  templateUrl: './dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {}
