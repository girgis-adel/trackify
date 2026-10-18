import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { apiBase } from './api';

export interface Dsp {
  id: string;
  name: string;
}
export interface Bootstrap {
  genres: string[];
  trackStatuses: string[];
  distributionStatuses: string[];
  dsps: Dsp[];
  demoEnabled: boolean;
  guestBrowsingEnabled: boolean;
}

@Injectable({ providedIn: 'root' })
export class BootstrapService {
  private readonly http = inject(HttpClient);
  private readonly dataState = signal<Bootstrap | null>(null);
  private pending: Promise<Bootstrap> | null = null;
  readonly data = this.dataState.asReadonly();

  load(force = false): Promise<Bootstrap> {
    const existing = this.dataState();
    if (existing && !force) return Promise.resolve(existing);
    if (!this.pending) {
      this.pending = firstValueFrom(this.http.get<Bootstrap>(`${apiBase}/ui/bootstrap`))
        .then((value) => {
          this.dataState.set(value);
          return value;
        })
        .finally(() => {
          this.pending = null;
        });
    }
    return this.pending;
  }
}
