import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { apiErrorMessage } from '../../core/api';
import { AuthService } from '../../core/auth/auth.service';
import { BootstrapService } from '../../core/bootstrap.service';
import { ArtistService } from '../artists/artist.service';
import { Track, TrackService } from '../tracks/track.service';

@Component({
  selector: 'app-dashboard-page',
  imports: [DatePipe, MatButtonModule, MatIconModule, RouterLink],
  template: `
    <header class="page-heading">
      <div>
        <p class="eyebrow accent-text">YOUR MUSIC ECOSYSTEM</p>
        <h1>
          {{
            auth.user()
              ? 'Good to see you, ' + auth.user()!.displayName + '.'
              : 'Welcome to Trackify.'
          }}
        </h1>
        <p class="muted">A clear view of the catalog and where each release stands.</p>
      </div>
      <a mat-flat-button class="primary-action" routerLink="/tracks"
        >Explore tracks <mat-icon>arrow_forward</mat-icon></a
      >
    </header>
    <section class="hero-panel">
      <div class="hero-copy">
        <div class="eyebrow">CATALOG IN MOTION</div>
        <h2>Every release<br /><em>finds its way.</em></h2>
        <p>From first draft to every platform, keep the music moving.</p>
        <a routerLink="/tracks" class="hero-link"
          >View catalog <mat-icon>arrow_forward</mat-icon></a
        >
      </div>
      <div class="hero-art" aria-hidden="true">
        <div class="record-ring ring-1"></div>
        <div class="record-ring ring-2"></div>
        <div class="record-ring ring-3"></div>
        <div class="record-center"></div>
      </div>
    </section>
    @if (loading()) {
      <div class="metric-grid">
        @for (i of [1, 2, 3, 4]; track i) {
          <div class="skeleton skeleton-metric"></div>
        }
      </div>
      <div class="skeleton skeleton-list"></div>
    } @else if (error()) {
      <div class="state-panel error-state">
        <mat-icon>wifi_off</mat-icon>
        <h2>We couldn't load the overview</h2>
        <p>{{ error() }}</p>
        <button mat-stroked-button (click)="load()">Try again</button>
      </div>
    } @else {
      <div class="metric-grid">
        <div class="metric-card">
          <span class="metric-icon violet"><mat-icon>library_music</mat-icon></span
          ><span class="metric-number">{{ tracks().length }}</span
          ><span class="metric-label">Total tracks</span>
        </div>
        <div class="metric-card">
          <span class="metric-icon coral"><mat-icon>groups</mat-icon></span
          ><span class="metric-number">{{ artistCount() }}</span
          ><span class="metric-label">Artists</span>
        </div>
        <div class="metric-card">
          <span class="metric-icon gold"><mat-icon>schedule</mat-icon></span
          ><span class="metric-number">{{ count('Submitted') }}</span
          ><span class="metric-label">Submitted tracks</span>
        </div>
        <div class="metric-card">
          <span class="metric-icon teal"><mat-icon>public</mat-icon></span
          ><span class="metric-number">{{ count('Distributed') }}</span
          ><span class="metric-label">Distributed tracks</span>
        </div>
      </div>
      <section class="section-block">
        <div class="section-heading">
          <div>
            <p class="eyebrow accent-text">THE CATALOG</p>
            <h2>Tracks to explore</h2>
          </div>
          <a routerLink="/tracks" class="section-link"
            >See all tracks <mat-icon>arrow_forward</mat-icon></a
          >
        </div>
        @if (tracks().length) {
          <div class="track-list">
            @for (track of tracks().slice(0, 5); track track.id; let i = $index) {
              <a class="track-row" [routerLink]="['/tracks', track.id]"
                ><span class="track-number">{{ (i + 1).toString().padStart(2, '0') }}</span
                ><span class="cover-art cover-{{ i % 4 }}"><mat-icon>music_note</mat-icon></span
                ><span class="track-main"
                  ><strong>{{ track.title }}</strong
                  ><small>{{ track.genre }} · {{ track.isrc }}</small></span
                ><span class="track-date">{{
                  track.releaseDate ? (track.releaseDate | date: 'mediumDate') : 'No release date'
                }}</span
                ><span class="status-pill" [class]="'status-pill ' + track.status.toLowerCase()">{{
                  track.status
                }}</span
                ><mat-icon class="row-arrow">arrow_outward</mat-icon></a
              >
            }
          </div>
        } @else {
          <div class="state-panel">
            <mat-icon>music_off</mat-icon>
            <h3>No tracks yet</h3>
            <p>Tracks will appear here when the catalog has entries.</p>
          </div>
        }
      </section>
      <section class="quick-grid">
        <a class="quick-card" routerLink="/artists"
          ><mat-icon>groups</mat-icon><strong>Meet the artists</strong
          ><span>Explore every artist in the roster</span
          ><mat-icon class="quick-arrow">arrow_forward</mat-icon></a
        ><a class="quick-card" routerLink="/dsps"
          ><mat-icon>hub</mat-icon><strong>Distribution network</strong
          ><span>See the supported music platforms</span
          ><mat-icon class="quick-arrow">arrow_forward</mat-icon></a
        >
      </section>
    }
  `,
})
export class DashboardPage implements OnInit {
  readonly auth = inject(AuthService);
  private readonly trackService = inject(TrackService);
  private readonly artistService = inject(ArtistService);
  private readonly bootstrap = inject(BootstrapService);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly tracks = signal<Track[]>([]);
  readonly artistCount = signal(0);

  ngOnInit(): void {
    void this.load();
  }
  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      const [tracks, artists] = await Promise.all([
        firstValueFrom(this.trackService.list()),
        firstValueFrom(this.artistService.list()),
      ]);
      this.tracks.set(tracks);
      this.artistCount.set(artists.length);
      void this.bootstrap.load().catch(() => undefined);
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }
  count(status: string): number {
    return this.tracks().filter((track) => track.status === status).length;
  }
}
