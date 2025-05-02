import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UserPortalRoutingModule } from './user-portal-routing.module';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { ParkingCarouselComponent } from './components/parking-carousel/parking-carousel.component';
import { AllocationComponent } from './components/allocation/allocation.component';

@NgModule({
  imports: [
    CommonModule,
    UserPortalRoutingModule,
    DashboardComponent,
    ParkingCarouselComponent,
  ],
  declarations: [
    AllocationComponent
  ],
})
export class UserPortalModule {}
