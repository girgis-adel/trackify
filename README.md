# Trackify

Trackify is a track management assessment application. Its ASP.NET Core API handles artists, tracks, digital service providers (DSPs), distribution status, Identity users, and JWT authorization. The Angular Material frontend provides an overview, catalog, demo sign-in, and the management flows supported by the API.

## Quick start with Docker

You need Git and a running Docker Engine with Docker Compose (Docker Desktop on Windows or macOS). Allow at least 4 GB of memory for the stack, particularly SQL Server. No local .NET, Node.js, or SQL Server installation is needed.

1. Clone the repository and enter its root:

   ```bash
   git clone https://github.com/girgis-adel/trackify.git
   cd trackify
   ```

2. Copy `.env.example` to `.env` (`cp .env.example .env` on macOS/Linux or `Copy-Item .env.example .env` in PowerShell). Set `MSSQL_SA_PASSWORD`, `APP_JWT_SECRET_KEY`, and `DEMO_SUPERADMIN_PASSWORD` to separate strong values. The SQL password must meet SQL Server complexity rules; avoid semicolons because it is also used in a connection string. Use at least 32 random characters for the JWT key. `.env` is ignored by Git and excluded from Docker builds.

3. Start everything from the repository root:

   ```bash
   docker compose up -d --build
   docker compose ps
   ```

   The first run downloads images, builds .NET and Angular, creates the SQL volume, applies EF Core migrations, and seeds the demo database. Wait until all four services report `healthy`; SQL Server and the first migration may take several minutes.

4. Open **http://localhost:8080** (or the `FRONTEND_PORT` set in `.env`). Select one of the demo profiles shown on the sign-in page and sign in. Profiles come from the real API and use its normal JWT login flow.

5. Use `docker compose logs -f` to watch startup and `docker compose down` to stop the stack. Ordinary shutdown keeps SQL data.

This is an isolated local demo with publicly known demo profiles. Keep its ports bound to localhost; it is not a production deployment.

## Technology stack

The backend targets .NET 10 and uses ASP.NET Core Web API, EF Core with SQL Server, ASP.NET Core Identity, Redis, and FusionCache. The frontend uses Angular 22 and Angular Material. Docker Compose runs those services and serves the production Angular build with Nginx.

## Stack and ports

| Compose service | Purpose | Container port | Host access |
| --- | --- | --- | --- |
| `frontend` | Nginx serves Angular and proxies `/api/*` | 8080 | `http://localhost:8080` by default |
| `api` | ASP.NET Core Web API | 8080 | Internal only, through frontend `/api/*` |
| `sqlserver` | SQL Server 2022 Developer, persisted in `sqlserver-data` | 1433 | `127.0.0.1:14333` by default, for local development |
| `redis` | Redis cache and FusionCache backplane | 6379 | `127.0.0.1:16379` by default, for local development |

Host bindings use `127.0.0.1`. `FRONTEND_PORT`, `SQLSERVER_PORT`, and `REDIS_PORT` can be changed in `.env` if a default port is occupied. The API uses Compose hostnames `sqlserver` and `redis`; the browser uses only its own origin. Nginx preserves the `/api` prefix and passes authentication headers, methods, bodies, status codes, and API errors. Angular routes refresh through an SPA fallback; API failures do not become Angular HTML.

## Configuration, migrations, and demo data

| `.env` variable | Required | Use |
| --- | --- | --- |
| `MSSQL_SA_PASSWORD` | Yes | SQL Server `sa` password and API SQL connections |
| `APP_JWT_SECRET_KEY` | Yes | API JWT signing key; at least 32 random characters |
| `DEMO_SUPERADMIN_PASSWORD` | Yes | Seeded assessment owner password; distinct from public demo credentials |
| `FRONTEND_PORT` | No | Frontend host port, default `8080` |
| `SQLSERVER_PORT` | No | SQL Server loopback host port, default `14333` |
| `REDIS_PORT` | No | Redis loopback host port, default `16379` |

Compose sets both API SQL connection strings, the Redis address, JWT key, and demo settings through environment variables. Demo mode selects dedicated `TrackifyDemo`, separate from the ordinary `TrackifyApp` database. The API calls `MigrateAsync()` and its idempotent initializer on startup; it seeds roles, the configured assessment owner, demo users, artists, tracks, DSPs, and distributions. No manual database creation or `dotnet ef` command is needed. Normal restarts keep data in `sqlserver-data`. The public demo profiles are returned by `/api/v1/demo/accounts` when demo mode is enabled and displayed by the UI.

The API container uses ASP.NET Core `Development` so HTTP behind the local Nginx proxy does not redirect to an unavailable container HTTPS endpoint. Its Docker build excludes the tracked `appsettings.Development.json`; local machine connection values and credentials are not baked into the image. Do not use this demo configuration on a public host.

## Local application development

Install the .NET 10 SDK and a Node.js version supported by Angular 22 (Node 24.15+ is a convenient choice). Use the existing npm lockfile. Prepare `.env` as above, then start only infrastructure from the repository root:

```bash
docker compose up -d sqlserver redis
docker compose ps
```

In a PowerShell terminal at the repository root, load the simple `KEY=value` entries from `.env` and override the local API connections:

```powershell
Get-Content .env | Where-Object { $_ -match '^[A-Z_]+=' } | ForEach-Object {
    $name, $value = $_ -split '=', 2
    [Environment]::SetEnvironmentVariable($name, $value, 'Process')
}
$env:ConnectionStrings__SqlServerDatabaseContext = "Server=127.0.0.1,$env:SQLSERVER_PORT;Database=TrackifyApp;User Id=sa;Password=$env:MSSQL_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True"
$env:ConnectionStrings__DemoSqlServerDatabaseContext = "Server=127.0.0.1,$env:SQLSERVER_PORT;Database=TrackifyDemo;User Id=sa;Password=$env:MSSQL_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True"
$env:ConnectionStrings__Redis = "127.0.0.1:$env:REDIS_PORT"
$env:AppJwtBearerOptions__SecretKey = $env:APP_JWT_SECRET_KEY
$env:SuperAdminUser__UserName = 'assessment.owner'
$env:SuperAdminUser__Email = 'owner@trackify.local'
$env:SuperAdminUser__Password = $env:DEMO_SUPERADMIN_PASSWORD
$env:SuperAdminUser__PhoneNumber = ''
$env:DemoOptions__Enabled = 'true'
dotnet run --project backend/src/Api/Api.csproj --launch-profile https
```

In another terminal:

```bash
cd frontend
npm ci
npm start
```

Open **http://localhost:4200**. Angular's development proxy forwards the same relative `/api/v1` calls to `http://localhost:5277`, the API's HTTP launch-profile address. The API's `https://localhost:7288` address is also available for direct local requests. No TypeScript URL edit or CORS setting is needed. On Windows, stop a running API process before rebuilding to release its `Api/bin` DLLs.

On macOS/Linux, use this Bash equivalent instead of the PowerShell API block:

```bash
while IFS='=' read -r key value; do
  value=${value%$'\r'}
  case "$key" in
    MSSQL_SA_PASSWORD|APP_JWT_SECRET_KEY|DEMO_SUPERADMIN_PASSWORD|SQLSERVER_PORT|REDIS_PORT) export "$key=$value" ;;
  esac
done < .env
export ConnectionStrings__SqlServerDatabaseContext="Server=127.0.0.1,${SQLSERVER_PORT:-14333};Database=TrackifyApp;User Id=sa;Password=$MSSQL_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True"
export ConnectionStrings__DemoSqlServerDatabaseContext="Server=127.0.0.1,${SQLSERVER_PORT:-14333};Database=TrackifyDemo;User Id=sa;Password=$MSSQL_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True"
export ConnectionStrings__Redis="127.0.0.1:${REDIS_PORT:-16379}"
export AppJwtBearerOptions__SecretKey="$APP_JWT_SECRET_KEY"
export SuperAdminUser__UserName='assessment.owner'
export SuperAdminUser__Email='owner@trackify.local'
export SuperAdminUser__Password="$DEMO_SUPERADMIN_PASSWORD"
export SuperAdminUser__PhoneNumber=''
export DemoOptions__Enabled=true
dotnet run --project backend/src/Api/Api.csproj --launch-profile https
```

Tracked development appsettings can remain unchanged because environment variables override them. The API still migrates and seeds on startup.

## Useful commands

```bash
docker compose ps
docker compose logs -f
docker compose logs -f api
docker compose up -d --build api frontend
docker compose restart api
docker compose down
```

`docker compose down` keeps the named database volume. Removing that volume deletes local SQL data; do so only when you intentionally want a fresh database.

## Troubleshooting

- **Docker Engine unavailable:** start Docker Desktop (or the Docker daemon), then retry `docker compose ps`.
- **Host port occupied:** change `FRONTEND_PORT`, `SQLSERVER_PORT`, or `REDIS_PORT` in `.env`, then rerun Compose. Internal service ports stay the same.
- **SQL Server slow or unhealthy:** allow first initialization several minutes and inspect `docker compose logs sqlserver`. Check Docker memory and the SQL password complexity. A semicolon in the password breaks the API connection string.
- **Host-run API times out connecting to SQL:** use `127.0.0.1` in its connection strings as shown above; Compose publishes the infrastructure ports on IPv4 loopback. Inside Docker, use the service names instead.
- **API not healthy:** inspect `docker compose logs api`. Check SQL and Redis health, the three required secrets, and JWT key length. Its health check queries database-backed `/api/v1/ui/bootstrap`.
- **Overview cannot reach API:** open `http://localhost:8080/api/v1/ui/bootstrap` (or your frontend port); it should return JSON. A `502` points to API startup; use `docker compose ps` and `docker compose logs api`. Local `npm start` instead needs the API on port `5277`.
- **Deep Angular link shows 404:** open it on the frontend port, not the API. Nginx handles SPA routes; `/api/*` never uses the SPA fallback.
- **Missing demo data or login failure:** confirm demo mode is enabled and inspect API migration/seed logs. Use profiles shown by the UI. Changing the JWT key invalidates existing tokens; sign in again. Persisted accounts keep their original password if you later change the owner password in `.env`.
- **CORS errors in local development:** use `http://localhost:4200` and its `/api` proxy rather than calling the API directly from the browser across origins.

The solution is `trackify.slnx`. Backend projects are under `backend/src`, and Angular is under `frontend`. See [frontend/README.md](frontend/README.md) for frontend-specific details.
