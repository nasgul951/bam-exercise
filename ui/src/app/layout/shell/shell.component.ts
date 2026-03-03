import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { RouterModule } from '@angular/router';
import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
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
      <mat-sidenav
        #sidenav
        [mode]="isMobile ? 'over' : 'side'"
        [opened]="!isMobile"
        class="w-56 p-2"
      >
        <mat-nav-list>
          <a mat-list-item routerLink="/persons" routerLinkActive="bg-indigo-100" (click)="closeSidenavOnMobile()">
            <mat-icon matListItemIcon>people</mat-icon>
            <span matListItemTitle>Persons</span>
          </a>
          <a mat-list-item routerLink="/logs" routerLinkActive="bg-indigo-100" (click)="closeSidenavOnMobile()">
            <mat-icon matListItemIcon>list_alt</mat-icon>
            <span matListItemTitle>Logs</span>
          </a>
        </mat-nav-list>
      </mat-sidenav>

      <mat-sidenav-content class="flex flex-col">
        <mat-toolbar color="primary" class="flex justify-between">
          <div class="flex items-center gap-2">
            @if (isMobile) {
              <button mat-icon-button (click)="sidenav.toggle()" aria-label="Toggle navigation">
                <mat-icon>menu</mat-icon>
              </button>
            }
            <span>ACTS Database</span>
          </div>
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
export class ShellComponent implements OnInit {
  @ViewChild('sidenav') sidenav!: MatSidenav;

  private readonly authService = inject(AuthService);
  private readonly breakpointObserver = inject(BreakpointObserver);

  isMobile = false;

  ngOnInit(): void {
    this.breakpointObserver
      .observe([Breakpoints.Handset, Breakpoints.TabletPortrait])
      .subscribe(result => {
        this.isMobile = result.matches;
      });
  }

  closeSidenavOnMobile(): void {
    if (this.isMobile) {
      this.sidenav.close();
    }
  }

  logout(): void {
    this.authService.logout();
  }
}
