import { Component, OnInit, inject, signal } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { RouterLink } from '@angular/router';
import { firstValueFrom } from 'rxjs';
import { apiErrorMessage } from '../../core/api';
import { Dsp } from '../../core/bootstrap.service';
import { DspService } from './dsp.service';

@Component({
  selector: 'app-dsps-page',
  imports: [MatButtonModule, MatIconModule, RouterLink],
  template: `
    <header class="page-heading">
      <div>
        <p class="eyebrow accent-text">BEYOND THE STUDIO</p>
        <h1>Platforms</h1>
        <p class="muted">The digital destinations available for distribution.</p>
      </div>
      <a mat-stroked-button routerLink="/tracks">Explore tracks</a>
    </header>
    @if (loading()) {
      <div class="platform-grid">
        @for (i of [1, 2, 3, 4]; track i) {
          <div class="skeleton skeleton-platform"></div>
        }
      </div>
    } @else if (error()) {
      <div class="state-panel error-state">
        <mat-icon>wifi_off</mat-icon>
        <h2>Platforms couldn't load</h2>
        <p>{{ error() }}</p>
        <button mat-stroked-button (click)="load()">Try again</button>
      </div>
    } @else if (!dsps().length) {
      <div class="state-panel">
        <mat-icon>hub</mat-icon>
        <h2>No platforms yet</h2>
        <p>Supported platforms will appear here when configured.</p>
      </div>
    } @else {
      <div class="platform-grid">
        @for (dsp of dsps(); track dsp.id; let i = $index) {
          <div class="platform-card">
            <div class="platform-icon platform-{{ i % 4 }}"><mat-icon>graphic_eq</mat-icon></div>
            <div>
              <span class="eyebrow accent-text">DISTRIBUTION PLATFORM</span>
              <h2>{{ dsp.name }}</h2>
              <p>Available for track submissions</p>
            </div>
            <mat-icon class="platform-check">check_circle</mat-icon>
          </div>
        }
      </div>
    }
  `,
})
export class DspsPage implements OnInit {
  private readonly service = inject(DspService);
  readonly dsps = signal<Dsp[]>([]);
  readonly loading = signal(true);
  readonly error = signal('');
  ngOnInit(): void {
    void this.load();
  }
  async load(): Promise<void> {
    this.loading.set(true);
    this.error.set('');
    try {
      this.dsps.set(await firstValueFrom(this.service.list()));
    } catch (error) {
      this.error.set(apiErrorMessage(error));
    } finally {
      this.loading.set(false);
    }
  }
}
