import { Routes } from '@angular/router';

import { managerGuard } from './core/auth/manager.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login.page').then((m) => m.LoginPage),
  },
  {
    path: '',
    loadComponent: () => import('./layout/shell').then((m) => m.Shell),
    children: [
      {
        path: '',
        pathMatch: 'full',
        loadComponent: () =>
          import('./features/dashboard/dashboard.page').then((m) => m.DashboardPage),
      },
      {
        path: 'tracks',
        loadComponent: () => import('./features/tracks/tracks.page').then((m) => m.TracksPage),
      },
      {
        path: 'tracks/:id',
        loadComponent: () =>
          import('./features/tracks/track-detail.page').then((m) => m.TrackDetailPage),
      },
      {
        path: 'artists',
        loadComponent: () => import('./features/artists/artists.page').then((m) => m.ArtistsPage),
      },
      {
        path: 'artists/new',
        canActivate: [managerGuard],
        loadComponent: () =>
          import('./features/artists/artist-create.page').then((m) => m.ArtistCreatePage),
      },
      {
        path: 'artists/:id',
        loadComponent: () =>
          import('./features/artists/artist-detail.page').then((m) => m.ArtistDetailPage),
      },
      {
        path: 'dsps',
        loadComponent: () => import('./features/dsps/dsps.page').then((m) => m.DspsPage),
      },
      {
        path: 'forbidden',
        loadComponent: () => import('./shared/message.page').then((m) => m.MessagePage),
        data: { kind: 'forbidden' },
      },
      {
        path: 'unavailable',
        loadComponent: () => import('./shared/message.page').then((m) => m.MessagePage),
        data: { kind: 'unavailable' },
      },
      {
        path: '**',
        loadComponent: () => import('./shared/message.page').then((m) => m.MessagePage),
        data: { kind: 'missing' },
      },
    ],
  },
];
