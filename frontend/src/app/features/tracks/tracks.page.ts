import { DatePipe } from '@angular/common';
import { Component, DestroyRef, OnInit, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { RouterLink } from '@angular/router';
import { catchError, combineLatest, firstValueFrom, of, startWith, switchMap, tap } from 'rxjs';
import { apiErrorMessage } from '../../core/api';
import { AuthService } from '../../core/auth/auth.service';
import { BootstrapService } from '../../core/bootstrap.service';
import { Artist, ArtistService } from '../artists/artist.service';
import { Track, TrackService } from './track.service';

@Component({
  selector: 'app-tracks-page',
  imports: [
    DatePipe,
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatSelectModule,
    RouterLink,
  ],
  template: `
    <header class="page-heading">
      <div>
        <p class="eyebrow accent-text">THE MUSIC LIBRARY</p>
        <h1>Tracks</h1>
        <p class="muted">Follow each release from draft to distribution.</p>
      </div>
      @if (auth.canManageCatalog()) {
        <a mat-flat-button class="primary-action" routerLink="/tracks/new"
          ><mat-icon>add</mat-icon> Add track</a
        >
      }
      <span class="heading-count">{{ tracks().length }} tracks</span>
    </header>
    <div class="filter-bar">
      <mat-icon>tune</mat-icon><span>Filter catalog</span>
      <mat-form-field appearance="outline" subscriptSizing="dynamic"
        ><mat-label>Artist</mat-label
        ><mat-select [formControl]="artistId"
          ><mat-option value="">All artists</mat-option>
          @for (artist of artists(); track artist.id) {
            <mat-option [value]="artist.id">{{ artist.name }}</mat-option>
          }
        </mat-select></mat-form-field
      >
      <mat-form-field appearance="outline" subscriptSizing="dynamic"
        ><mat-label>Genre</mat-label
        ><mat-select [formControl]="genre"
          ><mat-option value="">All genres</mat-option>
          @for (item of bootstrap.data()?.genres ?? []; track item) {
            <mat-option [value]="item">{{ item }}</mat-option>
          }
        </mat-select></mat-form-field
      >
      <mat-form-field appearance="outline" subscriptSizing="dynamic"
        ><mat-label>Status</mat-label
        ><mat-select [formControl]="status"
          ><mat-option value="">All statuses</mat-option>
          @for (item of bootstrap.data()?.trackStatuses ?? []; track item) {
            <mat-option [value]="item">{{ item }}</mat-option>
          }
        </mat-select></mat-form-field
      >
      @if (artistId.value || genre.value || status.value) {
        <button mat-button (click)="clearFilters()">Clear</button>
      }
    </div>
    @if (optionsError()) {
      <div class="inline-error">
        Filter options unavailable: {{ optionsError() }}
        <button mat-button (click)="loadOptions()">Retry</button>
      </div>
    }
    @if (artistOptionsError()) {
      <div class="inline-error">
        Artist options unavailable: {{ artistOptionsError() }}
        <button mat-button (click)="loadArtists()">Retry</button>
      </div>
    }
    @if (loading()) {
      <div class="track-list">
        @for (i of [1, 2, 3, 4, 5, 6]; track i) {
          <div class="skeleton skeleton-row"></div>
        }
      </div>
    } @else if (error()) {
      <div class="state-panel error-state">
        <mat-icon>wifi_off</mat-icon>
        <h2>Tracks couldn't load</h2>
        <p>{{ error() }}</p>
        <button mat-stroked-button (click)="retry()">Try again</button>
      </div>
    } @else if (!tracks().length) {
      <div class="state-panel">
        <mat-icon>music_off</mat-icon>
        <h2>
          {{
            artistId.value || genre.value || status.value
              ? 'No matching tracks'
              : 'The catalog is empty'
          }}
        </h2>
        <p>
          {{
            artistId.value || genre.value || status.value
              ? 'Try another genre or status.'
              : 'Tracks will appear here when they are available.'
          }}
        </p>
        @if (artistId.value || genre.value || status.value) {
          <button mat-stroked-button (click)="clearFilters()">Clear filters</button>
        }
      </div>
    } @else {
      <div class="track-list catalog-list">
        @for (track of tracks(); track track.id; let i = $index) {
          <a class="track-row" [routerLink]="['/tracks', track.id]"
            ><span class="track-number">{{ (i + 1).toString().padStart(2, '0') }}</span
            ><span class="cover-art cover-{{ i % 4 }}"><mat-icon>music_note</mat-icon></span
            ><span class="track-main"
              ><strong>{{ track.title }}</strong
              ><small>{{ track.artistName }} · {{ track.genre }} · {{ track.isrc }}</small></span
            ><span class="track-date">{{
              track.releaseDate ? (track.releaseDate | date: 'mediumDate') : 'Unscheduled'
            }}</span
            ><span class="status-pill" [class]="'status-pill ' + track.status.toLowerCase()">{{
              track.status
            }}</span
            ><mat-icon class="row-arrow">arrow_outward</mat-icon></a
          >
        }
      </div>
    }
  `,
})
export class TracksPage implements OnInit {
  readonly auth = inject(AuthService);
  readonly bootstrap = inject(BootstrapService);
  private readonly service = inject(TrackService);
  private readonly artistService = inject(ArtistService);
  private readonly destroyRef = inject(DestroyRef);
  readonly artistId = new FormControl('', { nonNullable: true });
  readonly genre = new FormControl('', { nonNullable: true });
  readonly status = new FormControl('', { nonNullable: true });
  readonly tracks = signal<Track[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly optionsError = signal('');
  readonly artists = signal<Artist[]>([]);
  readonly artistOptionsError = signal('');
  private readonly retryCount = new FormControl(0, { nonNullable: true });

  ngOnInit(): void {
    void this.loadOptions();
    void this.loadArtists();
    combineLatest([
      this.artistId.valueChanges.pipe(startWith(this.artistId.value)),
      this.genre.valueChanges.pipe(startWith(this.genre.value)),
      this.status.valueChanges.pipe(startWith(this.status.value)),
      this.retryCount.valueChanges.pipe(startWith(0)),
    ])
      .pipe(
        tap(() => {
          this.loading.set(true);
          this.error.set('');
        }),
        switchMap(([artistId, genre, status]) =>
          this.service.list({ artistId, genre, status }).pipe(
            catchError((error) => {
              this.error.set(apiErrorMessage(error));
              return of([] as Track[]);
            }),
          ),
        ),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe((tracks) => {
        this.tracks.set(tracks);
        this.loading.set(false);
      });
  }
  clearFilters(): void {
    this.artistId.setValue('', { emitEvent: false });
    this.genre.setValue('', { emitEvent: false });
    this.status.setValue('');
  }
  retry(): void {
    this.retryCount.setValue(this.retryCount.value + 1);
    void this.loadOptions();
    void this.loadArtists();
  }
  async loadOptions(): Promise<void> {
    this.optionsError.set('');
    try {
      await this.bootstrap.load(true);
    } catch (error) {
      this.optionsError.set(apiErrorMessage(error));
    }
  }
  async loadArtists(): Promise<void> {
    this.artistOptionsError.set('');
    try {
      this.artists.set(await firstValueFrom(this.artistService.list()));
    } catch (error) {
      this.artistOptionsError.set(apiErrorMessage(error));
    }
  }
}
