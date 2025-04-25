import { Component } from '@angular/core';
import { RouterModule } from '@angular/router'; // Import RouterModule

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: true, // Make component standalone
  imports: [RouterModule], // Import RouterModule for router-outlet
})
export class AppComponent {
  title = 'trivium-parking-app';
}
