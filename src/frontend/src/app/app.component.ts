import { Component } from '@angular/core';
import { RouterModule } from '@angular/router';
import { PullToRefreshDirective } from './core/directives/pull-to-refresh.directive';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  standalone: true,
  imports: [RouterModule, PullToRefreshDirective],
})
export class AppComponent {
  title = 'trivium-parking-app';
}
