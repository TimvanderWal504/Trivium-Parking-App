import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './component/header/header.component';
import { HomeredirectComponent } from './components/homeredirect/homeredirect.component';
import { ModalComponent } from './models/modal/modal.component';



@NgModule({
  declarations: [
    HeaderComponent,
    HomeredirectComponent,
    ModalComponent
  ],
  imports: [
    CommonModule
  ]
})
export class CoreModule { }
