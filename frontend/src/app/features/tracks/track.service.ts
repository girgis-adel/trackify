import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiBase } from '../../core/api';

export interface Track {
  id: string;
  isrc: string;
  title: string;
  releaseDate: string | null;
  genre: string;
  status: string;
}

export interface Distribution {
  id: string;
  dspId: string;
  dspName: string;
  submittedAt: string;
  status: string;
  goLiveDate: string | null;
}

export interface TrackDetails extends Track {
  distributions: Distribution[];
}
export interface TrackFilter {
  genre?: string;
  status?: string;
}

@Injectable({ providedIn: 'root' })
export class TrackService {
  private readonly http = inject(HttpClient);

  list(filter: TrackFilter = {}): Observable<Track[]> {
    let params = new HttpParams();
    if (filter.genre) params = params.set('genre', filter.genre);
    if (filter.status) params = params.set('status', filter.status);
    return this.http.get<Track[]>(`${apiBase}/tracks`, { params });
  }

  get(id: string): Observable<TrackDetails> {
    return this.http.get<TrackDetails>(`${apiBase}/tracks/${encodeURIComponent(id)}`);
  }

  updateStatus(id: string, status: string): Observable<Track> {
    return this.http.patch<Track>(`${apiBase}/tracks/${encodeURIComponent(id)}/status`, { status });
  }

  distribute(id: string, dspIds: string[]): Observable<Distribution[]> {
    return this.http.post<Distribution[]>(
      `${apiBase}/tracks/${encodeURIComponent(id)}/distribute`,
      { dspIds },
    );
  }
}
