import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { NotificationsService } from '../../core/notifications.service';
import { ArtistService } from '../artists/artist.service';
import { TrackCreatePage } from './track-create.page';
import { TrackService } from './track.service';

describe('TrackCreatePage', () => {
  const create = vi.fn();
  const navigate = vi.fn();

  beforeEach(() => {
    create.mockReset();
    navigate.mockReset();
    navigate.mockResolvedValue(true);
    TestBed.configureTestingModule({
      providers: [
        { provide: TrackService, useValue: { create } },
        { provide: ArtistService, useValue: { list: () => of([]) } },
        { provide: Router, useValue: { navigate } },
        { provide: NotificationsService, useValue: { success: vi.fn() } },
      ],
    });
  });

  function page(): TrackCreatePage {
    const component = TestBed.runInInjectionContext(() => new TrackCreatePage());
    component.artists.set([{ id: 'artist-1', name: 'Artist', email: 'artist@example.test', country: null }]);
    component.artistsLoading.set(false);
    return component;
  }

  it('submits normalized track fields and navigates to the created track', async () => {
    const component = page();
    component.form.setValue({
      artistId: 'artist-1',
      title: '  New Release  ',
      isrc: 'ustfy2600009',
      genre: '  Jazz  ',
      releaseDate: '',
    });
    create.mockReturnValue(of({ id: 'track-1' }));

    await component.submit();

    expect(create).toHaveBeenCalledWith({
      artistId: 'artist-1',
      title: 'New Release',
      isrc: 'USTFY2600009',
      genre: 'Jazz',
      releaseDate: null,
    });
    expect(navigate).toHaveBeenCalledWith(['/tracks', 'track-1']);
  });

  it('rejects whitespace titles before sending a request', async () => {
    const component = page();
    component.form.setValue({
      artistId: 'artist-1',
      title: '   ',
      isrc: 'USTFY2600009',
      genre: 'Jazz',
      releaseDate: '',
    });

    await component.submit();

    expect(create).not.toHaveBeenCalled();
    expect(component.form.controls.title.hasError('required')).toBe(true);
  });

  it('shows server validation on the matching field', async () => {
    const component = page();
    component.form.setValue({
      artistId: 'artist-1',
      title: 'New Release',
      isrc: 'USTFY2600009',
      genre: 'Jazz',
      releaseDate: '',
    });
    create.mockReturnValue(
      throwError(() => ({ error: { errors: { Title: ['Title is unavailable.'] } } })),
    );

    await component.submit();

    expect(component.form.controls.title.getError('server')).toBe('Title is unavailable.');
  });
});
