import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config'; // Import app config
import { AppComponent } from './app/app.component'; // Import root component

bootstrapApplication(AppComponent, appConfig) // Use bootstrapApplication
  .catch((err) => console.error(err));
