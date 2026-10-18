# Trackify Studio frontend

Angular 22 and Angular Material frontend for the existing Trackify API. The UI uses the backend's real v1 contracts; no frontend account, track, or distribution data is mocked.

## Local development

From the repository root, make sure SQL Server and Redis are available at the addresses configured in `backend/src/Api/appsettings.Development.json`. The development configuration currently enables demo mode and uses its separate demo SQL database. Starting the API runs its existing EF Core migration and seed initializer against that configured database.

In one PowerShell terminal:

```powershell
dotnet run --project backend/src/Api/Api.csproj --launch-profile https
```

In another terminal:

```powershell
cd frontend
npm ci
npm start
```

Open `http://localhost:4200`. The API `https` launch profile listens on both `https://localhost:7288` and `http://localhost:5277`. The Angular development proxy forwards `/api` to the loopback HTTP endpoint. In Development, the API does not redirect that proxied request to HTTPS; deployed environments retain HTTPS redirection. This avoids cross-origin redirects and does not bypass certificate validation. Keep the API process running while using the frontend. If the API port changes, update `proxy.conf.json`. For a deployed production build, serve `/api/v1` from the same origin through the host's reverse proxy.

On Windows, stop the running API with `Ctrl+C` before rebuilding backend projects. The running process can lock DLLs in `backend/src/Api/bin/Debug/net10.0`. Restart it after the build finishes.

Demo profiles are fetched from `/api/v1/demo/accounts` only when `/api/v1/ui/bootstrap` reports that demo mode is enabled. A demo selection submits the credentials to `/api/v1/identity/login`. The browser keeps only the access token and expiration in session storage; there is no refresh endpoint. Guest browsing uses public endpoints without a token. Admin and SuperAdmin actions are based on the authenticated user's `canManageCatalog` capability and are enforced again by the API.

The backend currently supports listing and viewing tracks, filtering by genre and status, status updates, DSP submissions, listing and viewing artists, creating an artist profile for an existing Identity user, and listing DSPs. There is no create-track or list-users API, and tracks have no artist relationship. The frontend does not invent those workflows.

## Build

```powershell
cd frontend
npm run build
```

Build output is written to `frontend/dist/frontend`.
