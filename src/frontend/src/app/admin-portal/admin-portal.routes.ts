import { Routes } from '@angular/router';
import { UserCreateComponent } from './components/user-create/user-create.component';

export const ADMIN_PORTAL_ROUTES: Routes = [
  {
    path: 'users/create',
    component: UserCreateComponent,
  },
  // TODO: Add other routes for admin portal components (e.g., user list, parking config)
  // Example:
  // {
  //   path: '', // Default route for '/admin'
  //   component: AdminDashboardComponent
  // },
  // {
  //   path: 'users',
  //   component: UserListComponent
  // }
];
