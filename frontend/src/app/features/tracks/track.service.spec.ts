import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { TrackService } from './track.service';

describe('TrackService', () => {
  let service: TrackService;
  let http: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(TrackService);
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('sends artist, genre and status filters to the API together', () => {
    service.list({ artistId: 'artist-1', genre: 'Jazz', status: 'Draft' }).subscribe();

    const request = http.expectOne((candidate) => candidate.url === '/api/v1/tracks');
    expect(request.request.method).toBe('GET');
    expect(request.request.params.get('artistId')).toBe('artist-1');
    expect(request.request.params.get('genre')).toBe('Jazz');
    expect(request.request.params.get('status')).toBe('Draft');
    request.flush([]);
  });

  it('posts the artist relationship when creating a track', () => {
    const body = {
      artistId: 'artist-1',
      isrc: 'USTFY2600009',
      title: 'New Release',
      genre: 'Jazz',
      releaseDate: null,
    };
    service.create(body).subscribe();

    const request = http.expectOne('/api/v1/tracks');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(body);
    request.flush({ id: 'track-1', ...body, status: 'Draft', artistName: 'Artist' });
  });
});
