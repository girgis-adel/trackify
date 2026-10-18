import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { apiErrorMessage, fieldErrors } from '../../core/api';
import { NotificationsService } from '../../core/notifications.service';
import { Artist, ArtistService } from '../artists/artist.service';
import { TrackService } from './track.service';

@Component({
  selector: 'app-track-create-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSelectModule,
    RouterLink,
  ],
  template: `
    <a class="back-link" routerLink="/tracks"><mat-icon>arrow_back</mat-icon> Back to tracks</a>
    <header class="page-heading">
      <div>
        <p class="eyebrow accent-text">CATALOG MANAGEMENT</p>
        <h1>Add a track</h1>
        <p class="muted">Create a draft track for an artist in the catalog.</p>
      </div>
    </header>
    <section class="surface-card form-card">
      @if (artistsLoading()) {
        <div class="skeleton skeleton-list"></div>
      } @else if (artistsError()) {
        <div class="inline-error" role="alert">
          Artist options unavailable: {{ artistsError() }}
          <button mat-button type="button" (click)="loadArtists()">Retry</button>
        </div>
      } @else if (!artists().length) {
        <div class="notice-box">
          <mat-icon>info</mat-icon>
          <p>No artists are available. <a routerLink="/artists/new">Add an artist</a> first.</p>
        </div>
      }
      <form [formGroup]="form" (ngSubmit)="submit()" class="form-stack">
        <mat-form-field appearance="outline">
          <mat-label>Artist</mat-label>
          <mat-select formControlName="artistId">
            @for (artist of artists(); track artist.id) {
              <mat-option [value]="artist.id">{{ artist.name }}</mat-option>
            }
          </mat-select>
          @if (form.controls.artistId.invalid && form.controls.artistId.touched) {
            <mat-error>{{
              form.controls.artistId.getError('server') || 'Choose an artist.'
            }}</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Track title</mat-label>
          <input matInput formControlName="title" maxlength="256" />
          @if (form.controls.title.invalid && form.controls.title.touched) {
            <mat-error>{{
              form.controls.title.getError('server') || 'Enter a title (max 256 characters).'
            }}</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>ISRC</mat-label>
          <input matInput formControlName="isrc" maxlength="12" autocomplete="off" />
          <mat-hint>12 characters, for example USTFY2600009.</mat-hint>
          @if (form.controls.isrc.invalid && form.controls.isrc.touched) {
            <mat-error>{{
              form.controls.isrc.getError('server') || 'Enter a valid 12-character ISRC.'
            }}</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Genre</mat-label>
          <input matInput formControlName="genre" maxlength="128" />
          @if (form.controls.genre.invalid && form.controls.genre.touched) {
            <mat-error>{{
              form.controls.genre.getError('server') || 'Enter a genre (max 128 characters).'
            }}</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline">
          <mat-label>Release date (optional)</mat-label>
          <input matInput type="date" formControlName="releaseDate" />
        </mat-form-field>
        @if (error()) {
          <p class="inline-error" role="alert">{{ error() }}</p>
        }
        <div class="form-actions">
          <a mat-button routerLink="/tracks">Cancel</a>
          <button
            mat-flat-button
            class="primary-action"
            type="submit"
            [disabled]="busy() || artistsLoading() || !!artistsError() || !artists().length"
          >
            {{ busy() ? 'Creating…' : 'Create track' }}
          </button>
        </div>
      </form>
    </section>
  `,
})
export class TrackCreatePage implements OnInit {
  private readonly service = inject(TrackService);
  private readonly artistService = inject(ArtistService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationsService);

  readonly artists = signal<Artist[]>([]);
  readonly artistsLoading = signal(true);
  readonly artistsError = signal('');
  readonly busy = signal(false);
  readonly error = signal('');
  readonly form = new FormGroup({
    artistId: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    title: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(256)],
    }),
    isrc: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.pattern(/^[a-zA-Z]{2}[a-zA-Z0-9]{3}[0-9]{7}$/)],
    }),
    genre: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(128)],
    }),
    releaseDate: new FormControl('', { nonNullable: true }),
  });

  ngOnInit(): void {
    void this.loadArtists();
  }

  async loadArtists(): Promise<void> {
    this.artistsLoading.set(true);
    this.artistsError.set('');
    try {
      this.artists.set(await firstValueFrom(this.artistService.list()));
    } catch (error) {
      this.artistsError.set(apiErrorMessage(error));
    } finally {
      this.artistsLoading.set(false);
    }
  }

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const value = this.form.getRawValue();
    if (!value.title.trim() || !value.genre.trim()) {
      if (!value.title.trim()) this.form.controls.title.setErrors({ required: true });
      if (!value.genre.trim()) this.form.controls.genre.setErrors({ required: true });
      this.form.markAllAsTouched();
      return;
    }
    if (this.busy() || this.artistsLoading() || this.artistsError()) return;
    this.busy.set(true);
    this.error.set('');
    try {
      const track = await firstValueFrom(
        this.service.create({
          artistId: value.artistId,
          title: value.title.trim(),
          isrc: value.isrc.trim().toUpperCase(),
          genre: value.genre.trim(),
          releaseDate: value.releaseDate ? `${value.releaseDate}T00:00:00` : null,
        }),
      );
      this.notifications.success('Draft track created.');
      await this.router.navigate(['/tracks', track.id]);
    } catch (error) {
      for (const [key, messages] of Object.entries(fieldErrors(error))) {
        const control = this.form.get(key[0].toLowerCase() + key.slice(1));
        if (control) control.setErrors({ server: messages.join(' ') });
      }
      this.form.markAllAsTouched();
      this.error.set(apiErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }
}
