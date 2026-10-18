import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../core/api';
import { DemoAccount } from '../../core/auth/auth.models';
import { AuthService } from '../../core/auth/auth.service';
import { BootstrapService } from '../../core/bootstrap.service';

@Component({
  selector: 'app-login-page',
  imports: [
    ReactiveFormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatProgressBarModule,
    RouterLink,
  ],
  template: `
    <div class="login-layout">
      <section class="login-promo">
        <a class="brand" routerLink="/"
          ><span class="brand-mark"><span></span></span
          ><span>trackify<small>STUDIO</small></span></a
        >
        <div class="promo-content">
          <div class="eyebrow"><span class="live-dot"></span> THE CREATOR WORKSPACE</div>
          <h1>Every track<br />has a <em>journey.</em></h1>
          <p>
            One place to explore your catalog, see where music is headed, and keep every release in
            motion.
          </p>
          <div class="sound-bars" aria-hidden="true">
            @for (bar of bars; track $index) {
              <span [style.height.px]="bar"></span>
            }
          </div>
        </div>
        <span class="promo-foot">YOUR MUSIC. EVERYWHERE.</span>
      </section>
      <section class="login-panel">
        <div class="login-inner">
          <p class="eyebrow accent-text">WELCOME BACK</p>
          <h2>Step into the studio.</h2>
          <p class="muted">
            Sign in to manage your music, or explore the public catalog as a guest.
          </p>
          <form [formGroup]="form" (ngSubmit)="signIn()" class="form-stack">
            <mat-form-field appearance="outline"
              ><mat-label>Email address</mat-label
              ><input matInput type="email" autocomplete="username" formControlName="email" />
              @if (form.controls.email.invalid && form.controls.email.touched) {
                <mat-error>Enter a valid email address.</mat-error>
              }
            </mat-form-field>
            <mat-form-field appearance="outline"
              ><mat-label>Password</mat-label
              ><input
                matInput
                [type]="showPassword() ? 'text' : 'password'"
                autocomplete="current-password"
                formControlName="password"
              />
              <button
                mat-icon-button
                matSuffix
                type="button"
                [attr.aria-label]="showPassword() ? 'Hide password' : 'Show password'"
                (click)="showPassword.set(!showPassword())"
              >
                <mat-icon>{{ showPassword() ? 'visibility_off' : 'visibility' }}</mat-icon>
              </button>
              @if (form.controls.password.invalid && form.controls.password.touched) {
                <mat-error>Password is required.</mat-error>
              }
            </mat-form-field>
            @if (loginError()) {
              <div class="inline-error" role="alert">{{ loginError() }}</div>
            }
            <button
              mat-flat-button
              class="primary-action full-width"
              type="submit"
              [disabled]="busy()"
            >
              {{ busy() ? 'Signing in…' : 'Sign in' }} <mat-icon>arrow_forward</mat-icon>
            </button>
          </form>
          <div class="divider"><span>OR</span></div>
          <a mat-stroked-button class="full-width guest-button" [routerLink]="guestDestination()"
            >Continue as guest</a
          >
          @if (loadingDemo()) {
            <div class="demo-loading">
              <mat-progress-bar mode="indeterminate" /><span>Checking demo accounts…</span>
            </div>
          }
          @if (demoAccounts().length) {
            <div class="demo-heading">
              <div>
                <p class="eyebrow accent-text">TRY THE EXPERIENCE</p>
                <h3>Choose a demo profile</h3>
              </div>
              <span class="demo-count">{{ demoAccounts().length }} profiles</span>
            </div>
            <div class="demo-grid">
              @for (account of demoAccounts(); track account.email) {
                <button
                  type="button"
                  class="demo-card"
                  [disabled]="busy()"
                  (click)="signInDemo(account)"
                >
                  <span class="demo-avatar">{{ initials(account.displayName) }}</span>
                  <span
                    ><strong>{{ account.displayName }}</strong
                    ><small>{{ account.persona }}</small
                    ><span class="demo-description">{{ account.description }}</span></span
                  >
                  <mat-icon>arrow_outward</mat-icon>
                </button>
              }
            </div>
          }
          @if (demoError()) {
            <p class="subtle-error">
              {{ demoError() }}
              <button type="button" class="text-button" (click)="loadDemo()">Retry</button>
            </p>
          }
        </div>
      </section>
    </div>
  `,
})
export class LoginPage implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly bootstrap = inject(BootstrapService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  readonly form = new FormGroup({
    email: new FormControl('', {
      nonNullable: true,
      validators: [Validators.required, Validators.email, Validators.maxLength(256)],
    }),
    password: new FormControl('', { nonNullable: true, validators: [Validators.required] }),
  });
  readonly bars = [16, 31, 23, 50, 38, 62, 44, 70, 49, 33, 56, 73, 41, 26, 53, 31, 17];
  readonly busy = signal(false);
  readonly showPassword = signal(false);
  readonly loginError = signal('');
  readonly demoError = signal('');
  readonly loadingDemo = signal(true);
  readonly demoAccounts = signal<DemoAccount[]>([]);

  ngOnInit(): void {
    void this.auth
      .restore()
      .then(() => {
        if (this.auth.isAuthenticated()) void this.router.navigateByUrl(this.destination());
      })
      .catch(() => undefined);
    this.loadDemo();
  }

  guestDestination(): string {
    return '/';
  }
  signIn(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { email, password } = this.form.getRawValue();
    void this.authenticate(email, password);
  }
  signInDemo(account: DemoAccount): void {
    void this.authenticate(account.email, account.password);
  }

  async loadDemo(): Promise<void> {
    this.loadingDemo.set(true);
    this.demoError.set('');
    try {
      const bootstrap = await this.bootstrap.load();
      this.demoAccounts.set(bootstrap.demoEnabled ? await this.auth.getDemoAccounts() : []);
    } catch (error) {
      if ((error as { status?: number }).status !== 404) this.demoError.set(apiErrorMessage(error));
    } finally {
      this.loadingDemo.set(false);
    }
  }

  initials(name: string): string {
    return name
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0]?.toUpperCase() ?? '')
      .join('');
  }

  private async authenticate(email: string, password: string): Promise<void> {
    if (this.busy()) return;
    this.busy.set(true);
    this.loginError.set('');
    try {
      await this.auth.signIn(email, password);
      this.form.controls.password.reset();
      this.demoAccounts.set([]);
      await this.router.navigateByUrl(this.destination());
    } catch (error) {
      this.loginError.set(apiErrorMessage(error));
    } finally {
      this.busy.set(false);
    }
  }

  private destination(): string {
    const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
    return returnUrl?.startsWith('/') && !returnUrl.startsWith('//') && returnUrl !== '/login'
      ? returnUrl
      : '/';
  }
}
