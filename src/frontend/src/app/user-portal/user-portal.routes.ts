import { Routes } from '@angular/router';
import { DashboardComponent } from './components/dashboard/dashboard.component'; // Import DashboardComponent

// Routes specific to the User Portal feature area
export const USER_PORTAL_ROUTES: Routes = [
  {
    path: '', // Default route for '/portal'
    component: DashboardComponent,
  },
  // TODO: Add other routes for user portal components (e.g., calendar)
  // {
  //   path: 'calendar',
  //   component: CalendarComponent
  // }
];
