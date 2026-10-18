import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiBase } from '../../core/api';

export interface Artist {
  id: string;
  name: string;
  email: string;
  country: string | null;
}
export interface CreateArtist {
  userId: string;
  name: string;
  country: string | null;
}

@Injectable({ providedIn: 'root' })
export class ArtistService {
  private readonly http = inject(HttpClient);

  list(): Observable<Artist[]> {
    return this.http.get<Artist[]>(`${apiBase}/artists`);
  }
  get(id: string): Observable<Artist> {
    return this.http.get<Artist>(`${apiBase}/artists/${encodeURIComponent(id)}`);
  }
  create(request: CreateArtist): Observable<Artist> {
    return this.http.post<Artist>(`${apiBase}/artists`, request);
  }
}
