import { BreakpointObserver } from '@angular/cdk/layout';
import { Component, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth/auth.service';
import { BootstrapService } from '../core/bootstrap.service';

@Component({
  selector: 'app-shell',
  imports: [
    MatButtonModule,
    MatIconModule,
    MatSidenavModule,
    MatToolbarModule,
    RouterLink,
    RouterLinkActive,
    RouterOutlet,
  ],
  template: `
    <mat-sidenav-container class="app-frame">
      <mat-sidenav
        #drawer
        class="app-sidebar"
        [mode]="compact() ? 'over' : 'side'"
        [opened]="sidebarOpen()"
        (closedStart)="sidebarOpen.set(false)"
      >
        <a class="brand" routerLink="/" (click)="closeOnMobile()" aria-label="Trackify home">
          <span class="brand-mark"><span></span></span><span>trackify<small>STUDIO</small></span>
        </a>
        <div class="nav-caption">WORKSPACE</div>
        <nav class="side-nav" aria-label="Main navigation">
          <a
            routerLink="/"
            routerLinkActive="active"
            [routerLinkActiveOptions]="{ exact: true }"
            (click)="closeOnMobile()"
            ><mat-icon>dashboard</mat-icon> Overview</a
          >
          <a routerLink="/tracks" routerLinkActive="active" (click)="closeOnMobile()"
            ><mat-icon>library_music</mat-icon> Tracks</a
          >
          <a routerLink="/artists" routerLinkActive="active" (click)="closeOnMobile()"
            ><mat-icon>groups</mat-icon> Artists</a
          >
          <a routerLink="/dsps" routerLinkActive="active" (click)="closeOnMobile()"
            ><mat-icon>hub</mat-icon> Platforms</a
          >
        </nav>
        <div class="sidebar-bottom">
          <span class="live-dot"></span> LIVE CATALOG
          <p>Music moves further together.</p>
        </div>
      </mat-sidenav>
      <mat-sidenav-content>
        <mat-toolbar class="topbar">
          @if (compact()) {
            <button mat-icon-button aria-label="Open menu" (click)="drawer.toggle()">
              <mat-icon>menu</mat-icon>
            </button>
          }
          <div class="topbar-title">
            Track management <span class="topbar-divider">/</span>
            <span class="muted">Workspace</span>
          </div>
          <span class="spacer"></span>
          @if (auth.user(); as user) {
            <div class="account-chip">
              <span class="avatar">{{ initials(user.displayName) }}</span
              ><span class="account-text"
                ><strong>{{ user.displayName }}</strong
                ><small>{{ user.roles.join(' · ') }}</small></span
              >
            </div>
            <button mat-icon-button aria-label="Sign out" title="Sign out" (click)="signOut()">
              <mat-icon>logout</mat-icon>
            </button>
          } @else {
            <span class="guest-label">Guest mode</span>
            <a mat-stroked-button routerLink="/login">Sign in</a>
          }
        </mat-toolbar>
        <main class="page-content"><router-outlet /></main>
      </mat-sidenav-content>
    </mat-sidenav-container>
  `,
})
export class Shell implements OnInit {
  readonly auth = inject(AuthService);
  private readonly bootstrap = inject(BootstrapService);
  private readonly router = inject(Router);
  readonly compact = signal(false);
  readonly sidebarOpen = signal(true);

  constructor() {
    inject(BreakpointObserver)
      .observe('(max-width: 900px)')
      .pipe(takeUntilDestroyed())
      .subscribe((result) => {
        this.compact.set(result.matches);
        this.sidebarOpen.set(!result.matches);
      });
  }

  ngOnInit(): void {
    void this.bootstrap.load().catch(() => undefined);
  }
  closeOnMobile(): void {
    if (this.compact()) this.sidebarOpen.set(false);
  }
  initials(name: string): string {
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }
  signOut(): void {
    this.auth.signOut();
    void this.router.navigateByUrl('/login');
  }
}
