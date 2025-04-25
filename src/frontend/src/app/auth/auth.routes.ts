import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';

// Routes specific to the Auth feature area
export const AUTH_ROUTES: Routes = [
  {
    path: '', // Default route for this lazy-loaded module (maps to '/auth')
    component: LoginComponent,
  },
  // Add other auth routes here later if needed (e.g., register)
  // { path: 'register', component: RegisterComponent },
];
