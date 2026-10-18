# Trackify — AI-Assisted Development Decisions

This document describes how I used AI during the Trackify assessment, what I owned personally, and how I reviewed and corrected the result.

## 1. What did AI generate, and what did I write or modify myself?

I developed the backend myself: the .NET solution structure, Clean Architecture layers, domain entities, interfaces and abstractions, EF Core persistence, application services, and API controllers. I also designed the authentication and authorization flow and the core track/distribution operations. I used AI primarily as a reviewer for **FluentValidation** rules and to improve validation messages so API errors would be clearer and more consistent.

For the Angular frontend, I created the project, initial layout, core components, and examples of how services should call the API. I then used AI to complete the remaining frontend views and integrations. Once the functional frontend was in place, I used AI again to refine its presentation into a Spotify-inspired music-management interface. I reviewed the generated work against the actual API contracts and adjusted the integration where needed.

AI helped me implement and iterate faster, but I remained responsible for the architecture, API behavior, integration decisions, and final code review.

## 2. What security issues did I find or introduce in AI-generated code? How did I handle them?

One risk I checked in the AI-assisted frontend was relying on **route guards or hidden management buttons as authorization**. Those checks improve the UI, but users can bypass client-side code and send requests directly to an API. I therefore checked that the backend enforces permissions independently: catalog-changing endpoints use JWT-based role authorization, while the frontend guard is only a usability measure. The Angular HTTP interceptor also attaches the bearer token only to the intended API requests, rather than indiscriminately to other URLs.

During the broader security review, I found a more serious issue in earlier sample-data code: demo users' **plain-text passwords had been added to ASP.NET Identity claims**. Credentials must never be stored in claims, especially when those claims could be exposed through token generation or other identity operations. I removed that behavior, added cleanup for existing `password` claims, and kept JWT creation limited to identity and role claims. The current implementation reflects that correction.

I also treated publicly documented demo accounts as development-only data. The Docker setup uses an isolated demo database, binds its exposed ports to localhost, and reads the SQL password, JWT signing key, and assessment-owner password from an untracked `.env` file. The tracked development configuration no longer contains a JWT signing key or admin credential, and demo mode is disabled by default and rejected outside Development. Any credentials reused from earlier commits should be rotated before using or exposing the application beyond an isolated local demonstration. The frontend's `sessionStorage` token is also accessible to JavaScript if an XSS vulnerability exists; I would reassess token storage and CSP for production.

## 3. What is one thing AI got wrong that I had to fix, and why?

In an early version of the AI-assisted artist-creation UI, the form asked for an **existing Identity user ID** to create an artist profile. That matched an early backend contract, but it was not a practical creation flow: a manager would have to know a database user ID before adding a new artist.

I corrected the workflow so the manager provides the artist's name and account details instead. The backend now creates the Identity user, assigns the Artist role, and creates the related artist profile in one database transaction. I updated the request DTO and validation rules, and the frontend form and service were changed to use the new contract. This is a good example of why generated UI must be evaluated as an end-to-end user flow, not just as code that compiles or matches an existing endpoint.

The correction is visible in [the artist-creation changes](https://github.com/girgis-adel/trackify/commit/86e4ebfbdfc901d0f07fc7291b997bf4f9807377); the earlier credential-claim cleanup is visible in [the backend implementation history](https://github.com/girgis-adel/trackify/commit/b381f97c4b226d0505e4eeb0900d77bbffa5d130).
