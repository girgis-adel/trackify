import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { apiBase } from '../../core/api';
import { Dsp } from '../../core/bootstrap.service';

@Injectable({ providedIn: 'root' })
export class DspService {
  private readonly http = inject(HttpClient);
  list(): Observable<Dsp[]> {
    return this.http.get<Dsp[]>(`${apiBase}/dsps`);
  }
}
