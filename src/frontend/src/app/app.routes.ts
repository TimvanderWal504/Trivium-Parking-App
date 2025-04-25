import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { HomeRedirectComponent } from './core/components/homeredirect/homeredirect.component';

export const routes: Routes = [
  { path: '', pathMatch: 'full', component: HomeRedirectComponent },
  {
    path: 'auth',
    loadChildren: () => import('./auth/auth.routes').then((m) => m.AUTH_ROUTES),
  },
  {
    path: 'portal',
    canActivate: [authGuard],
    loadChildren: () =>
      import('./user-portal/user-portal.routes').then(
        (m) => m.USER_PORTAL_ROUTES
      ),
  },
  {
    path: 'admin',
    loadChildren: () =>
      import('./admin-portal/admin-portal.routes').then(
        (m) => m.ADMIN_PORTAL_ROUTES
      ),
  },
  {
    path: '**',
    redirectTo: '',
  },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
