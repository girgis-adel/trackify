import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { apiErrorMessage } from '../../core/api';
import { Artist, ArtistService } from './artist.service';

@Component({
  selector: 'app-artist-detail-page',
  imports: [MatButtonModule, MatIconModule, RouterLink],
  template: `
    <a class="back-link" routerLink="/artists"><mat-icon>arrow_back</mat-icon> Back to artists</a>
    @if (loading()) {
      <div class="skeleton skeleton-detail"></div>
    } @else if (error()) {
      <div class="state-panel error-state">
        <mat-icon>person_off</mat-icon>
        <h2>{{ missing() ? 'Artist not found' : 'Artist unavailable' }}</h2>
        <p>{{ error() }}</p>
        <button mat-stroked-button (click)="load()">Try again</button>
      </div>
    } @else if (artist(); as item) {
      <div class="artist-profile">
        <div class="artist-profile-art">
          <span>{{ initials(item.name) }}</span>
        </div>
        <div class="artist-profile-body">
          <p class="eyebrow accent-text">ARTIST PROFILE</p>
          <h1>{{ item.name }}</h1>
          <p class="profile-location">
            <mat-icon>location_on</mat-icon>{{ item.country || 'Location not listed' }}
          </p>
          <div class="profile-contact">
            <span>CONTACT</span><a [href]="'mailto:' + item.email">{{ item.email }}</a>
          </div>
        </div>
      </div>
    }
  `,
})
export class ArtistDetailPage implements OnInit {
  private readonly service = inject(ArtistService);
  private readonly route = inject(ActivatedRoute);
  readonly artist = signal<Artist | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly missing = signal(false);
  ngOnInit(): void {
    void this.load();
  }
  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    this.missing.set(false);
    try {
      this.artist.set(
        await firstValueFrom(this.service.get(this.route.snapshot.paramMap.get('id') ?? '')),
      );
    } catch (error) {
      this.missing.set((error as { status?: number }).status === 404);
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
