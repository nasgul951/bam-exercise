import { Routes } from '@angular/router';
import { authGuard } from './core/auth/auth.guard';
import { LoginComponent } from './features/auth/login/login.component';
import { ShellComponent } from './layout/shell/shell.component';
import { PersonsListComponent } from './features/persons/persons-list/persons-list.component';
import { PersonDetailComponent } from './features/persons/person-detail/person-detail.component';
import { LogsListComponent } from './features/logs/logs-list/logs-list.component';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'persons', pathMatch: 'full' },
      { path: 'persons', component: PersonsListComponent },
      { path: 'persons/:name', component: PersonDetailComponent },
      { path: 'logs', component: LogsListComponent },
    ],
  },
  { path: '**', redirectTo: '' },
];
