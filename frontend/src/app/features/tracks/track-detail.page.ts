import { DatePipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { apiErrorMessage } from '../../core/api';
import { AuthService } from '../../core/auth/auth.service';
import { BootstrapService, Dsp } from '../../core/bootstrap.service';
import { NotificationsService } from '../../core/notifications.service';
import { TrackDetails, TrackService } from './track.service';

@Component({
  selector: 'app-track-detail-page',
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
    <a class="back-link" routerLink="/tracks"><mat-icon>arrow_back</mat-icon> Back to tracks</a>
    @if (loading()) {
      <div class="skeleton skeleton-detail"></div>
      <div class="skeleton skeleton-list"></div>
    } @else if (error()) {
      <div class="state-panel error-state">
        <mat-icon>{{ missing() ? 'music_off' : 'wifi_off' }}</mat-icon>
        <h2>{{ missing() ? 'Track not found' : 'Track unavailable' }}</h2>
        <p>{{ error() }}</p>
        <button mat-stroked-button (click)="load()">Try again</button>
      </div>
    } @else if (track(); as item) {
      <section class="detail-hero">
        <div class="detail-cover"><mat-icon>music_note</mat-icon></div>
        <div class="detail-intro">
          <p class="eyebrow accent-text">TRACK PROFILE</p>
          <h1>{{ item.title }}</h1>
          <p>
            <a [routerLink]="['/artists', item.artistId]">{{ item.artistName }}</a>
            <span class="dot-separator">·</span> {{ item.genre }}
            <span class="dot-separator">·</span> {{ item.isrc }}
          </p>
          <span class="status-pill" [class]="'status-pill ' + item.status.toLowerCase()">{{
            item.status
          }}</span>
        </div>
      </section>
      <div class="detail-grid">
        <section class="surface-card">
          <div class="section-heading compact-heading"><h2>Release details</h2></div>
          <dl class="info-list">
            <div>
              <dt>Artist</dt>
              <dd>
                <a [routerLink]="['/artists', item.artistId]">{{ item.artistName }}</a>
              </dd>
            </div>
            <div>
              <dt>ISRC</dt>
              <dd>{{ item.isrc }}</dd>
            </div>
            <div>
              <dt>Genre</dt>
              <dd>{{ item.genre }}</dd>
            </div>
            <div>
              <dt>Release date</dt>
              <dd>
                {{ item.releaseDate ? (item.releaseDate | date: 'longDate') : 'Not scheduled' }}
              </dd>
            </div>
            <div>
              <dt>Status</dt>
              <dd>{{ item.status }}</dd>
            </div>
          </dl>
        </section>
        <section class="surface-card">
          <div class="section-heading compact-heading">
            <h2>Distribution</h2>
            <span class="heading-count">{{ item.distributions.length }} platforms</span>
          </div>
          @if (item.distributions.length) {
            <div class="distribution-list">
              @for (distribution of item.distributions; track distribution.id) {
                <div class="distribution-row">
                  <span class="platform-mark"><mat-icon>graphic_eq</mat-icon></span
                  ><span class="distribution-name"
                    ><strong>{{ distribution.dspName }}</strong
                    ><small
                      >Submitted {{ distribution.submittedAt | date: 'mediumDate' }}
                      @if (distribution.goLiveDate) {
                        · Live since {{ distribution.goLiveDate | date: 'mediumDate' }}
                      }
                    </small></span
                  ><span
                    class="status-pill"
                    [class]="'status-pill ' + distribution.status.toLowerCase()"
                    >{{ distribution.status }}</span
                  >
                </div>
              }
            </div>
          } @else {
            <div class="small-empty">
              <mat-icon>hub</mat-icon>
              <p>This track has not been submitted to a platform.</p>
            </div>
          }
        </section>
      </div>
      @if (auth.canManageCatalog()) {
        @if (bootstrapLoading()) {
          <div class="skeleton skeleton-list"></div>
        } @else if (bootstrapError()) {
          <div class="state-panel error-state">
            <mat-icon>wifi_off</mat-icon>
            <h2>Catalog options unavailable</h2>
            <p>{{ bootstrapError() }}</p>
            <button mat-stroked-button (click)="loadBootstrap()">Try again</button>
          </div>
        } @else {
          <section class="management-grid">
            <div class="surface-card">
              <p class="eyebrow accent-text">CATALOG CONTROL</p>
              <h2>Update track status</h2>
              <p class="muted">Choose a status supported by the catalog.</p>
              <div class="action-form">
                <mat-form-field appearance="outline"
                  ><mat-label>Status</mat-label
                  ><mat-select [formControl]="statusControl">
                    @for (status of bootstrap.data()?.trackStatuses ?? []; track status) {
                      <mat-option [value]="status">{{ status }}</mat-option>
                    }
                  </mat-select></mat-form-field
                ><button
                  mat-flat-button
                  class="primary-action"
                  [disabled]="
                    statusBusy() || !statusControl.value || statusControl.value === item.status
                  "
                  (click)="saveStatus()"
                >
                  {{ statusBusy() ? 'Saving…' : 'Save status' }}
                </button>
              </div>
              @if (statusError()) {
                <p class="inline-error" role="alert">{{ statusError() }}</p>
              }
            </div>
            <div class="surface-card">
              <p class="eyebrow accent-text">DISTRIBUTION</p>
              <h2>Send to platforms</h2>
              <p class="muted">Only platforms without an existing submission are available.</p>
              @if (availableDsps().length) {
                <div class="action-form">
                  <mat-form-field appearance="outline"
                    ><mat-label>Choose platforms</mat-label
                    ><mat-select [formControl]="dspControl" multiple>
                      @for (dsp of availableDsps(); track dsp.id) {
                        <mat-option [value]="dsp.id">{{ dsp.name }}</mat-option>
                      }
                    </mat-select></mat-form-field
                  ><button
                    mat-flat-button
                    class="primary-action"
                    [disabled]="distributionBusy() || !dspControl.value.length"
                    (click)="submitDistribution()"
                  >
                    {{ distributionBusy() ? 'Submitting…' : 'Submit distribution' }}
                  </button>
                </div>
              } @else {
                <div class="small-empty">
                  <mat-icon>check_circle</mat-icon>
                  <p>All available platforms already have a submission.</p>
                </div>
              }
              @if (distributionError()) {
                <p class="inline-error" role="alert">{{ distributionError() }}</p>
              }
            </div>
          </section>
        }
      }
    }
  `,
})
export class TrackDetailPage implements OnInit {
  readonly auth = inject(AuthService);
  readonly bootstrap = inject(BootstrapService);
  private readonly service = inject(TrackService);
  private readonly route = inject(ActivatedRoute);
  private readonly notifications = inject(NotificationsService);
  readonly track = signal<TrackDetails | null>(null);
  readonly loading = signal(true);
  readonly error = signal('');
  readonly missing = signal(false);
  readonly statusBusy = signal(false);
  readonly distributionBusy = signal(false);
  readonly statusError = signal('');
  readonly distributionError = signal('');
  readonly bootstrapLoading = signal(true);
  readonly bootstrapError = signal('');
  readonly statusControl = new FormControl('', { nonNullable: true });
  readonly dspControl = new FormControl<string[]>([], { nonNullable: true });

  ngOnInit(): void {
    void this.load();
    void this.loadBootstrap();
  }
  async loadBootstrap(): Promise<void> {
    this.bootstrapLoading.set(true);
    this.bootstrapError.set('');
    try {
      await this.bootstrap.load(true);
    } catch (error) {
      this.bootstrapError.set(apiErrorMessage(error));
    } finally {
      this.bootstrapLoading.set(false);
    }
  }
  availableDsps(): Dsp[] {
    const used = new Set(this.track()?.distributions.map((item) => item.dspId) ?? []);
    return (this.bootstrap.data()?.dsps ?? []).filter((dsp) => !used.has(dsp.id));
  }
  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    this.missing.set(false);
    try {
      const item = await firstValueFrom(
        this.service.get(this.route.snapshot.paramMap.get('id') ?? ''),
      );
      this.track.set(item);
      this.statusControl.setValue(item.status);
    } catch (error) {
      this.missing.set((error as { status?: number }).status === 404);
      this.error.set(apiErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }
  async saveStatus(): Promise<void> {
    const item = this.track();
    if (!item || this.statusBusy() || !this.auth.canManageCatalog()) return;
    this.statusBusy.set(true);
    this.statusError.set('');
    try {
      const updated = await firstValueFrom(
        this.service.updateStatus(item.id, this.statusControl.value),
      );
      this.track.set({ ...item, status: updated.status });
      this.notifications.success('Track status updated.');
    } catch (error) {
      this.statusError.set(apiErrorMessage(error));
    } finally {
      this.statusBusy.set(false);
    }
  }
  async submitDistribution(): Promise<void> {
    const item = this.track();
    const dspIds = this.dspControl.value;
    if (!item || !dspIds.length || this.distributionBusy() || !this.auth.canManageCatalog()) return;
    this.distributionBusy.set(true);
    this.distributionError.set('');
    try {
      const created = await firstValueFrom(this.service.distribute(item.id, dspIds));
      this.track.set({
        ...item,
        status: item.status === 'Draft' ? 'Submitted' : item.status,
        distributions: [...item.distributions, ...created],
      });
      this.dspControl.setValue([]);
      this.notifications.success('Distribution submitted.');
    } catch (error) {
      this.distributionError.set(apiErrorMessage(error));
    } finally {
      this.distributionBusy.set(false);
    }
  }
}
