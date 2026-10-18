import { Component, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ActivatedRoute, RouterLink } from '@angular/router';

@Component({
  selector: 'app-message-page',
  imports: [MatButtonModule, MatIconModule, RouterLink],
  template: `
    <div class="message-page">
      <div class="message-icon">
        <mat-icon>{{ icon }}</mat-icon>
      </div>
      <p class="eyebrow accent-text">{{ code }}</p>
      <h1>{{ title }}</h1>
      <p class="muted">{{ detail }}</p>
      <a mat-flat-button class="primary-action" routerLink="/">Back to overview</a>
    </div>
  `,
})
export class MessagePage {
  private readonly kind = inject(ActivatedRoute).snapshot.data['kind'] as string;
  readonly code =
    this.kind === 'forbidden'
      ? '403 · RESTRICTED'
      : this.kind === 'unavailable'
        ? 'CONNECTION ISSUE'
        : '404 · NOT FOUND';
  readonly icon =
    this.kind === 'forbidden' ? 'lock' : this.kind === 'unavailable' ? 'wifi_off' : 'explore_off';
  readonly title =
    this.kind === 'forbidden'
      ? 'This space is restricted.'
      : this.kind === 'unavailable'
        ? 'The API is unavailable.'
        : 'That page is off the map.';
  readonly detail =
    this.kind === 'forbidden'
      ? 'Your account does not have catalog management access.'
      : this.kind === 'unavailable'
        ? 'Check the backend connection and try again.'
        : 'The page you requested does not exist.';
}
