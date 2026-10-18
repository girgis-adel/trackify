import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { apiErrorMessage, fieldErrors } from '../../core/api';
import { AuthService } from '../../core/auth/auth.service';
import { NotificationsService } from '../../core/notifications.service';
import { ArtistService } from './artist.service';

@Component({
  selector: 'app-artist-create-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    RouterLink,
  ],
  template: `
    <a class="back-link" routerLink="/artists"><mat-icon>arrow_back</mat-icon> Back to artists</a>
    <header class="page-heading">
      <div>
        <p class="eyebrow accent-text">CATALOG MANAGEMENT</p>
        <h1>Add an artist</h1>
        <p class="muted">Create a profile for an existing Trackify Identity user.</p>
      </div>
    </header>
    <section class="surface-card form-card">
      <div class="notice-box">
        <mat-icon>info</mat-icon>
        <p>
          This API requires the existing user's ID. It does not provide a user search or create-user
          endpoint. You can use your own ID from your signed-in user context when applicable.
        </p>
      </div>
      @if (auth.user(); as user) {
        <button
          mat-stroked-button
          type="button"
          class="self-id-button"
          (click)="form.controls.userId.setValue(user.id)"
        >
          Use my user ID
        </button>
      }
      <form [formGroup]="form" (ngSubmit)="submit()" class="form-stack">
        <mat-form-field appearance="outline"
          ><mat-label>Existing user ID</mat-label
          ><input matInput formControlName="userId" autocomplete="off" maxlength="450" />
          @if (form.controls.userId.invalid && form.controls.userId.touched) {
            <mat-error>{{
              form.controls.userId.getError('server') ||
                'A user ID is required (max 450 characters).'
            }}</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline"
          ><mat-label>Artist name</mat-label
          ><input matInput formControlName="name" maxlength="256" />
          @if (form.controls.name.invalid && form.controls.name.touched) {
            <mat-error>{{
              form.controls.name.getError('server') || 'A name is required (max 256 characters).'
            }}</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline"
          ><mat-label>Country (optional)</mat-label
          ><input matInput formControlName="country" maxlength="256" />
          @if (form.controls.country.invalid && form.controls.country.touched) {
            <mat-error>{{
              form.controls.country.getError('server') || 'Use at most 256 characters.'
            }}</mat-error>
          }
        </mat-form-field>
        @if (error()) {
          <p class="inline-error" role="alert">{{ error() }}</p>
        }
        <div class="form-actions">
          <a mat-button routerLink="/artists">Cancel</a
          ><button mat-flat-button class="primary-action" type="submit" [disabled]="busy()">
            {{ busy() ? 'Creating…' : 'Create artist' }}
          </button>
        </div>
      </form>
    </section>
  `,
})
export class ArtistCreatePage {
  readonly auth = inject(AuthService);
  private readonly service = inject(ArtistService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationsService);
  readonly form = new FormGroup({
    userId: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(450)],
    }),
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(256)],
    }),
    country: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(256)] }),
  });
  readonly busy = signal(false);
  readonly error = signal('');

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    if (!this.form.controls.userId.value.trim() || !this.form.controls.name.value.trim()) {
      if (!this.form.controls.userId.value.trim())
        this.form.controls.userId.setErrors({ required: true });
      if (!this.form.controls.name.value.trim())
        this.form.controls.name.setErrors({ required: true });
      this.form.markAllAsTouched();
      return;
    }
    if (this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    try {
      const value = this.form.getRawValue();
      const artist = await firstValueFrom(
        this.service.create({
          userId: value.userId.trim(),
          name: value.name.trim(),
          country: value.country.trim() || null,
        }),
      );
      this.notifications.success('Artist profile created.');
      await this.router.navigate(['/artists', artist.id]);
    } catch (error) {
      const errors = fieldErrors(error);
      for (const [key, messages] of Object.entries(errors)) {
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
