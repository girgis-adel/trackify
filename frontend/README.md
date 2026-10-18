# Trackify Studio frontend

Angular 22 and Angular Material frontend for the Trackify API. It uses real v1 endpoints and does not mock account, track, or distribution data. For full-stack setup, configuration, demo accounts, and troubleshooting, start with the [root README](../README.md).

## Full Docker stack

From the repository root, prepare `.env` from `.env.example`, set the three required secrets, then run:

```bash
docker compose up -d --build
docker compose ps
```

Open `http://localhost:8080` (or the configured `FRONTEND_PORT`). Nginx serves the production Angular build and forwards `/api/*` to the internal API container. Its SPA fallback supports refreshes on Angular routes and does not turn failed API calls into HTML pages. Select a demo profile on the sign-in page; the frontend retrieves profiles from the backend and sends the selected credentials through the real login endpoint.

## Local frontend development

Use Node.js 24.15+ (or another [Angular 22 supported Node version](https://angular.dev/reference/versions)) and npm. Start the API with the `https` launch profile as described in the root README, then from `frontend/` run:

```bash
npm ci
npm start
```

Open `http://localhost:4200`. The application calls the relative `/api/v1` path. Angular's development proxy forwards `/api` to `http://localhost:5277`, one of the API launch profile's addresses. The Docker Nginx configuration forwards that same path to `api:8080`; no source edits or browser CORS configuration are needed when switching workflows. Keep the API running while using the frontend.

On Windows, stop the running API with `Ctrl+C` before rebuilding backend projects; its process can lock DLLs in `backend/src/Api/bin/Debug/net10.0`.

## Build

```bash
npm ci
npm run build
```

The production browser assets are written to `frontend/dist/frontend/browser` and copied into the Nginx image. The frontend stores the access token and expiration in session storage, uses the API's real authorization capabilities, and does not invent endpoints for unsupported workflows.
