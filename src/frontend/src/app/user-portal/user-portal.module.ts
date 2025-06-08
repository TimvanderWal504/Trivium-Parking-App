import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UserPortalRoutingModule } from './user-portal-routing.module';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ParkingCarouselComponent } from './components/parking/parking-carousel/parking-carousel.component';
import { AllocationComponent } from './components/allocation/allocation.component';
import { LocationSelectorComponent } from './components/parking/location-selector/location-selector.component';
import { VisitorListComponent } from './components/parking/visitor-list/visitor-list.component';
import { ParkingRequestModalComponent } from './components/parking/parking-request-modal/parking-request-modal.component';
import { ParkingRequestInfoComponent } from './components/parking/parking-request-info/parking-request-info.component';

@NgModule({
  imports: [
    CommonModule,
    UserPortalRoutingModule,
    DashboardComponent,
    ParkingCarouselComponent,
    LocationSelectorComponent,
    AllocationComponent,
    VisitorListComponent,
    ParkingRequestModalComponent,
  ],
  declarations: [
    ParkingRequestInfoComponent
  ],
})
export class UserPortalModule {}
