import { Routes } from '@angular/router';
import { LandingComponent } from './components/landing/landing.component';
import { CompanyListComponent } from './components/company-list/company-list.component';
import { UserListComponent } from './components/user-list/user-list.component';
import { UserListClientComponent } from './components/user-list-client/user-list-client.component';
import { UserListPrecacheComponent } from './components/user-list-precache/user-list-precache.component';
import { UserListCursorComponent } from './components/user-list-cursor/user-list-cursor.component';

export const routes: Routes = [
  { path: '', component: LandingComponent },
  { path: 'companies', component: CompanyListComponent },
  { path: 'companies/:companyId/users', component: UserListComponent },
  { path: 'users', component: UserListComponent },
  // Client-side pagination routes
  { path: 'users-client', component: UserListClientComponent },
  { path: 'companies/:companyId/users-client', component: UserListClientComponent },
  // Precached pagination routes
  { path: 'users-precache', component: UserListPrecacheComponent },
  { path: 'companies/:companyId/users-precache', component: UserListPrecacheComponent },
  // Cursor pagination routes
  { path: 'users-cursor', component: UserListCursorComponent },
];
