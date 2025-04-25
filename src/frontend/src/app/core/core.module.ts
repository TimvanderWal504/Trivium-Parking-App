import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HeaderComponent } from './component/header/header.component';
import { HomeredirectComponent } from './components/homeredirect/homeredirect.component';



@NgModule({
  declarations: [
    HeaderComponent,
    HomeredirectComponent
  ],
  imports: [
    CommonModule
  ]
})
export class CoreModule { }
