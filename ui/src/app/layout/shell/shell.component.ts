import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatListModule } from '@angular/material/list';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../core/auth/auth.service';

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterModule,
    MatSidenavModule,
    MatToolbarModule,
    MatListModule,
    MatButtonModule,
    MatIconModule,
  ],
  template: `
    <mat-sidenav-container class="h-screen">
      <mat-sidenav mode="side" opened class="w-56 p-2">
        <mat-nav-list>
          <a mat-list-item routerLink="/persons" routerLinkActive="bg-indigo-100">
            <mat-icon matListItemIcon>people</mat-icon>
            <span matListItemTitle>Persons</span>
          </a>
          <a mat-list-item routerLink="/logs" routerLinkActive="bg-indigo-100">
            <mat-icon matListItemIcon>list_alt</mat-icon>
            <span matListItemTitle>Logs</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content class="flex flex-col">
        <mat-toolbar color="primary" class="flex justify-between">
          <span>Stargate</span>
          <button mat-button (click)="logout()">
            <mat-icon>logout</mat-icon>
            Logout
          </button>
        </mat-toolbar>
        <div class="flex-1 overflow-auto p-4">
          <router-outlet />
        </div>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
})
export class ShellComponent {
  private readonly authService = inject(AuthService);

  logout(): void {
    this.authService.logout();
  }
}
