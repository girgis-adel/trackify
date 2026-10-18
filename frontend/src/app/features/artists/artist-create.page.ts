import { Component, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { Router, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { apiErrorMessage, fieldErrors } from '../../core/api';
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
        <p class="muted">Create a sign-in account and artist profile together.</p>
      </div>
    </header>
    <section class="surface-card form-card">
      <form [formGroup]="form" (ngSubmit)="submit()" class="form-stack">
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
          ><mat-label>User name</mat-label
          ><input matInput formControlName="userName" autocomplete="username" maxlength="256" />
          @if (form.controls.userName.invalid && form.controls.userName.touched) {
            <mat-error>{{
              form.controls.userName.getError('server') ||
                'Use letters, numbers, and -._@+ (max 256 characters).'
            }}</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline"
          ><mat-label>Email</mat-label
          ><input
            matInput
            type="email"
            formControlName="email"
            autocomplete="email"
            maxlength="256"
          />
          @if (form.controls.email.invalid && form.controls.email.touched) {
            <mat-error>{{
              form.controls.email.getError('server') || 'Enter a valid email address.'
            }}</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline"
          ><mat-label>Password</mat-label
          ><input matInput type="password" formControlName="password" autocomplete="new-password" />
          <mat-hint>8+ characters with uppercase, lowercase, a number, and a symbol.</mat-hint>
          @if (form.controls.password.invalid && form.controls.password.touched) {
            <mat-error>{{
              form.controls.password.getError('server') || 'Use at least 8 characters.'
            }}</mat-error>
          }
        </mat-form-field>
        <mat-form-field appearance="outline"
          ><mat-label>Confirm password</mat-label
          ><input
            matInput
            type="password"
            formControlName="confirmPassword"
            autocomplete="new-password"
          />
          @if (form.controls.confirmPassword.invalid && form.controls.confirmPassword.touched) {
            <mat-error>{{
              form.controls.confirmPassword.getError('mismatch')
                ? 'Passwords do not match.'
                : 'Confirm the password.'
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
  private readonly service = inject(ArtistService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationsService);
  readonly form = new FormGroup({
    name: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.maxLength(256)],
    }),
    userName: new FormControl('', {
      nonNullable: true,
      validators: [
        Validators.required,
        Validators.maxLength(256),
        Validators.pattern(/^[a-zA-Z0-9._@+-]+$/),
      ],
    }),
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(256)],
    }),
    password: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.minLength(8)],
    }),
    confirmPassword: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
    country: new FormControl('', { nonNullable: true, validators: [Validators.maxLength(256)] }),
  });
  readonly busy = signal(false);
  readonly error = signal('');

  async submit(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    if (
      !this.form.controls.name.value.trim() ||
      !this.form.controls.userName.value.trim() ||
      !this.form.controls.email.value.trim()
    ) {
      if (!this.form.controls.name.value.trim())
        this.form.controls.name.setErrors({ required: true });
      if (!this.form.controls.userName.value.trim())
        this.form.controls.userName.setErrors({ required: true });
      if (!this.form.controls.email.value.trim())
        this.form.controls.email.setErrors({ required: true });
      this.form.markAllAsTouched();
      return;
    }
    if (this.form.controls.password.value !== this.form.controls.confirmPassword.value) {
      this.form.controls.confirmPassword.setErrors({ mismatch: true });
      this.form.controls.confirmPassword.markAsTouched();
      return;
    }
    if (this.busy()) return;
    this.busy.set(true);
    this.error.set('');
    try {
      const value = this.form.getRawValue();
      const artist = await firstValueFrom(
        this.service.create({
          userName: value.userName.trim(),
          email: value.email.trim(),
          password: value.password,
          name: value.name.trim(),
          country: value.country.trim() || null,
        }),
      );
      this.notifications.success('Artist account and profile created.');
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
