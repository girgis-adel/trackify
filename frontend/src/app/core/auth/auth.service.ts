import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { apiBase } from '../api';
import { DemoAccount, LoginResponse, User } from './auth.models';

interface StoredSession {
  accessToken: string;
  expiresAt: string;
}
const sessionKey = 'trackify.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly currentUser = signal<User | null>(null);
  private session: StoredSession | null = this.readSession();
  private restorePromise: Promise<void> | null = null;
  private expiryTimer: ReturnType<typeof setTimeout> | null = null;

  readonly user = this.currentUser.asReadonly();
  readonly isAuthenticated = computed(() => this.currentUser() !== null);
  readonly canManageCatalog = computed(() => this.currentUser()?.canManageCatalog === true);

  constructor() {
    this.scheduleExpiry();
  }

  token(): string | null {
    if (!this.session) return null;
    if (Date.parse(this.session.expiresAt) <= Date.now()) {
      this.signOut();
      return null;
    }
    return this.session.accessToken;
  }

  async restore(): Promise<void> {
    if (!this.token() || this.currentUser()) return;
    if (!this.restorePromise) {
      this.restorePromise = firstValueFrom(this.http.get<User>(`${apiBase}/users/me`))
        .then((user) => this.currentUser.set(user))
        .catch((error) => {
          if (error?.status === 401 || error?.status === 404) this.signOut();
          else throw error;
        })
        .finally(() => {
          this.restorePromise = null;
        });
    }
    return this.restorePromise;
  }

  async signIn(email: string, password: string): Promise<User> {
    const response = await firstValueFrom(
      this.http.post<LoginResponse>(`${apiBase}/identity/login`, { email, password }),
    );
    this.session = { accessToken: response.accessToken, expiresAt: response.expiresAt };
    try {
      sessionStorage.setItem(sessionKey, JSON.stringify(this.session));
    } catch {
      // The in-memory session remains usable when browser storage is unavailable.
    }
    this.currentUser.set(response.user);
    this.scheduleExpiry();
    return response.user;
  }

  getDemoAccounts(): Promise<DemoAccount[]> {
    return firstValueFrom(this.http.get<DemoAccount[]>(`${apiBase}/demo/accounts`));
  }

  signOut(): void {
    if (this.expiryTimer) clearTimeout(this.expiryTimer);
    this.expiryTimer = null;
    this.session = null;
    this.currentUser.set(null);
    try {
      sessionStorage.removeItem(sessionKey);
    } catch {
      // Storage may be disabled by the browser.
    }
  }

  private readSession(): StoredSession | null {
    try {
      const raw = sessionStorage.getItem(sessionKey);
      if (!raw) return null;
      const value = JSON.parse(raw) as StoredSession;
      return typeof value.accessToken === 'string' &&
        typeof value.expiresAt === 'string' &&
        Number.isFinite(Date.parse(value.expiresAt))
        ? value
        : null;
    } catch {
      return null;
    }
  }

  private scheduleExpiry(): void {
    if (this.expiryTimer) clearTimeout(this.expiryTimer);
    if (!this.session) return;
    const remaining = Date.parse(this.session.expiresAt) - Date.now();
    if (!(remaining > 0)) {
      this.signOut();
      return;
    }
    this.expiryTimer = setTimeout(() => this.scheduleExpiry(), Math.min(remaining, 2_147_483_647));
  }
}
