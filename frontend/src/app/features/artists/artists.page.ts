import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { apiErrorMessage } from '../../core/api';
import { AuthService } from '../../core/auth/auth.service';
import { Artist, ArtistService } from './artist.service';

@Component({
  selector: 'app-artists-page',
  imports: [MatButtonModule, MatIconModule, RouterLink],
  template: `
    <header class="page-heading">
      <div>
        <p class="eyebrow accent-text">THE PEOPLE BEHIND THE SOUND</p>
        <h1>Artists</h1>
        <p class="muted">Explore the creators in the Trackify roster.</p>
      </div>
      @if (auth.canManageCatalog()) {
        <a mat-flat-button class="primary-action" routerLink="/artists/new"
          ><mat-icon>person_add</mat-icon> Add artist</a
        >
      }
    </header>
    @if (loading()) {
      <div class="artist-grid">
        @for (i of [1, 2, 3, 4, 5, 6]; track i) {
          <div class="skeleton skeleton-artist"></div>
        }
      </div>
    } @else if (error()) {
      <div class="state-panel error-state">
        <mat-icon>wifi_off</mat-icon>
        <h2>Artists couldn't load</h2>
        <p>{{ error() }}</p>
        <button mat-stroked-button (click)="load()">Try again</button>
      </div>
    } @else if (!artists().length) {
      <div class="state-panel">
        <mat-icon>groups</mat-icon>
        <h2>No artists yet</h2>
        <p>Artist profiles will appear here when added.</p>
      </div>
    } @else {
      <div class="artist-grid">
        @for (artist of artists(); track artist.id; let i = $index) {
          <a class="artist-card" [routerLink]="['/artists', artist.id]"
            ><div class="artist-art artist-art-{{ i % 4 }}">
              <span>{{ initials(artist.name) }}</span>
            </div>
            <div class="artist-card-body">
              <span class="eyebrow accent-text">ARTIST PROFILE</span>
              <h2>{{ artist.name }}</h2>
              <p>{{ artist.country || 'Location not listed' }}</p>
              <span class="artist-link">View profile <mat-icon>arrow_forward</mat-icon></span>
            </div></a
          >
        }
      </div>
    }
  `,
})
export class ArtistsPage implements OnInit {
  readonly auth = inject(AuthService);
  private readonly service = inject(ArtistService);
  readonly artists = signal<Artist[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');

  ngOnInit(): void {
    void this.load();
  }
  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      this.artists.set(await firstValueFrom(this.service.list()));
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }
  initials(name: string): string {
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }
}
