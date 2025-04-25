import {
  ApplicationConfig,
  provideZoneChangeDetection,
  isDevMode,
} from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient } from '@angular/common/http'; // For making backend calls
import { provideServiceWorker } from '@angular/service-worker';
import { provideFirebaseApp, initializeApp } from '@angular/fire/app';
import { provideAuth, getAuth } from '@angular/fire/auth';
import { provideMessaging, getMessaging } from '@angular/fire/messaging';

import { routes } from './app.routes';
import { environment } from '../environments/environment'; // Use environment config

export const appConfig: ApplicationConfig = {
  providers: [
    // Core Angular providers
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes), // Provide application routes
    provideHttpClient(), // Provide HttpClient for API calls

    // Service Worker provider
    provideServiceWorker('ngsw-worker.js', {
      enabled: !isDevMode(),
      registrationStrategy: 'registerWhenStable:30000',
    }),

    // Firebase providers (using environment config)
    provideFirebaseApp(() => initializeApp(environment.firebase)),
    provideAuth(() => getAuth()),
    provideMessaging(() => getMessaging()),

    // TODO: Add application-specific service providers here later
    // Example: ParkingApiService, AuthService, etc.
  ],
};
