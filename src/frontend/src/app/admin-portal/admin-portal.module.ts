import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { AdminPortalRoutingModule } from './admin-portal-routing.module';
import { UserCreateComponent } from './components/user-create/user-create.component';

@NgModule({
  imports: [CommonModule, UserCreateComponent, AdminPortalRoutingModule],
})
export class AdminPortalModule {}
