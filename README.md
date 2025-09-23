# WORK IN PROGRESS — NOT PRODUCTION READY

# CoreAccess

**Last update:** 2025-09-24 (conversation snapshot)
**Note:** This Readme file is AI Generated, and not fully reviewed. I will replace it with a better, and self-written readme, when i hahe the time.

CoreAccess is a lightweight OpenID Connect identity provider and small Identity & Access Management (IAM) foundation implemented with ASP.NET Core and OpenIddict. It is container-first and includes a Vue 3 admin UI scaffold. The project is actively developed and many parts are still experimental or work-in-progress. This document summarizes architecture, current status, key design decisions, and recommended next steps so you can pick up development quickly.

## Quick status summary
- Containerized OpenIddict-based OIDC Identity Provider with its own login page (exposed).
- Startup supports creating dummy data if `COREACCESS_DEBUGMODE=true`.
- User / Role / Permission model is prototyped; API endpoints are defined but not fully complete.
- Admin UI (Vue 3 + Vite + TypeScript + Pinia + PrimeVue) scaffold exists; CRUD views are being implemented.
- Basic system logging via `ILogger` exists. Audit and access logging are planned but not fully implemented.
- SettingsService exists (DB + IMemoryCache) and can be used to provide runtime configuration (e.g. token lifetimes).
- Persistent cryptographic key handling: helper(s) exist to generate and persist RSA keys (PEM/JWK) on disk so OpenIddict has a signing key after restarts — important for production.
- PKCE support for SPA flows is implemented at the frontend design level (code_verifier / code_challenge).
- Plans and designs exist for: Service Accounts / Client Credentials, user-bound API keys, realtime log streaming to the Admin UI (SignalR/WebSocket), and a reusable `@coreaccess/client` SDK for frontends.

## Project goals
- Provide a small, self-hostable OIDC Identity Provider and Admin UI for managing identities, roles and permissions.
- Support both interactive users and machine/service identities (service accounts / client credentials).
- Make runtime configuration possible for critical auth parameters (token lifetimes, issuer, audience) without restart.
- Keep deployments container-friendly (persistent volume for DB and keys) and easy to run locally.

---

# Architecture & core concepts

## Layers
- **API / Controllers**: Thin HTTP layer that accepts request models and returns response DTOs.
- **Services**: Business logic layer. Accepts request models, coordinates repositories and other services, maps Entities to DTOs.
- **Repositories / DataAccess (EF Core)**: DB access layer. Works with EF Core entities (UserEntity, RoleEntity, PermissionEntity, ServiceAccount, Setting, etc.). Repositories accept and return entities only.
- **DTOs / Request models**:
  - Request models (Create/Update) are used for incoming data.
  - DTOs for API output — separate undetailed (list) and detailed DTOs for each resource.
- **Mapping**: Prefer explicit mapping (extension methods or small mappers). DTO constructors that accept Entities are allowed but keeping mapping in extension methods keeps DTOs dumb.

## Key design points
- Repositories deal only with EF Entities; Services map to/from DTOs.
- Services may call other services (e.g. TokenService calls UserService) when business logic requires it; avoid cyclic dependencies.
- Settings and secrets lifecycle:
  - Persist runtime-configurable settings in DB (Key-Value table).
  - Use an abstraction `ISecretProtector` (Data Protection or AES-GCM) to encrypt sensitive settings on write and decrypt on read.
  - Use `IMemoryCache` in `SettingsService` to avoid DB calls on hot paths (e.g. token issuance).
- Token issuance customization:
  - Hook into OpenIddict events (especially `ProcessSignInContext`) to set access/refresh token lifetimes, issuer, audiences and custom claims at runtime using values from `SettingsService`.

---

# Keys, certificates and persistence

## What must be persisted
- Signing keys (asymmetric) used to sign JWTs must be persistent. If the server generates ephemeral keys at each start, previously issued tokens will become unverifiable.
- Options for persistence:
  - Filesystem (persisted in a mounted container volume): JWK/PEM files with the private key.
  - External keystore (future): Azure Key Vault, AWS KMS, HashiCorp Vault.
  - Storing private keys in the DB is possible if desired (but current OpenIddict versions require you to implement persistence yourself).

## Recommended minimal setup
- Persist RSA keys on disk inside the container volume (e.g. `/var/coreaccess/keys`). Use a helper to generate a PEM (PKCS#8) or JWK containing the private key if not present, and load it into an `RsaSecurityKey` for OpenIddict at startup.
- For production: prefer a stable key location (volume mount) or integration with a real key management service. Always back up keys together with DB.

## Certificates vs. keys
- An X.509 certificate is a container for a public key and metadata; it can (optionally) contain a private key (e.g. PFX). It is convenient for enterprise setups and PKI integration.
- For CoreAccess, a raw RSA/ECDSA key persisted as PEM or JWK is usually sufficient.
- Use certificates when your deployment or operational practices rely on a PKI or certificate rotation through a CA.

---

# Settings, secrets and runtime configuration

## Settings storage (recommended)
- Table: `Settings` (Key, Value, EncryptedValue, IsSecret, UpdatedAt).
- `SettingsService`:
  - Reads/writes settings.
  - Protects secrets using `ISecretProtector`.
  - Caches values with `IMemoryCache` (TTL e.g. 5 minutes).
  - Provides typed helpers (e.g. `GetTokenLifetimeAsync()`).

## Secret protection
- Use ASP.NET Core Data Protection with a persistent key ring (mounted directory) for a straightforward solution.
- Alternative: AES-GCM with a master key from environment (`COREACCESS_MASTER_KEY`) — gives more control but requires careful key rotation and backup logic.
- Never log secret values. Record only that a secret changed (audit entry without the value).

## Dev vs Prod
- Dev:
  - DataProtection keys in-memory or local profile are OK.
  - SQLite + mounted folder for quick local iteration.
  - `COREACCESS_DEBUGMODE=true` can create dummy data.
- Prod:
  - Persist DataProtection key ring to a volume or use a dedicated secret manager.
  - Use Postgres and regular backups.
  - Persist signing keys and treat them like critical secrets to back up and rotate safely.

---

# Authentication & token flows

## User flows
- Authorization Code flow + PKCE for SPA (Admin UI).
- Password/Resource Owner flow available for scripts or tests (if configured).
- Token lifetimes and claims are set dynamically in the OpenIddict sign-in event handler based on SettingsService.

## Machine / service flows
- Client Credentials flow for service accounts (recommended for machine-to-machine).
- ServiceAccounts are separate entities, mapped to client_id + hashed client_secret and to roles/permissions.
- At token issuance for client credentials, populate the token claims from the ServiceAccount's roles/permissions (so your existing endpoint authorization decorator can remain unchanged).

## User-bound API keys
- Optionally offer user-bound API keys (Personal Access Tokens):
  - Generate a one-time display token for the user (store only its hash).
  - When presented in requests, map the key to the user, produce a ClaimsPrincipal and allow operations with the user's privileges.
  - Provide client/UI ergonomics to rotate/revoke keys and set optional expiry.

---

# API shape & DTO patterns

## REST resources (recommended)
- `GET /users` (paged, filter), `GET /users/{id}`, `POST /users`, `PUT /users/{id}`, `DELETE /users/{id}`
- `GET /roles`, `GET /roles/{id}`, `POST /roles`, `PUT /roles/{id}`, `DELETE /roles/{id}`
- `GET /permissions`, `POST /permissions` (if dynamic)
- Subresource endpoints for relationships:
  - `POST /users/{id}/roles`, `DELETE /users/{id}/roles/{roleId}`
  - `POST /roles/{id}/permissions`, `DELETE /roles/{id}/permissions/{permId}`

## DTOs
- Provide lightweight list DTOs (undetailed) for tables and detailed DTOs for resource views (include navigations, audit fields).
- Repositories return Entities. Services do the Entity ↔ DTO mapping. Controllers accept request DTOs and return response DTOs.

---

# Logging, audit and realtime stream

## System logs
- Use `ILogger<T>` and Serilog or the built-in logging to send structured logs to console (`stdout`) so the container orchestrator captures them.

## Audit & Access logs
- Store separate tables for `AuditLogs` and `AccessLogs` in DB:
  - `AuditLogs`: (Id, Timestamp, ActorUserId, Action, TargetId, Details JSON)
  - `AccessLogs`: (Id, Timestamp, IpAddress, UserId, Action, Details)
- Use these tables for Admin UI views for forensic and compliance workflows.

## Realtime log streaming
- Use a custom `ILoggerProvider` that forwards logs to a SignalR hub (or WebSocket) to support live log streaming in the Admin UI without writing all logs into the DB.
- Optionally: a configuration toggle to enable/disable live streaming in production.

---

# Admin UI (frontend)

## Stack
- Vue 3 + Vite + TypeScript
- Pinia for state
- PrimeVue for fast UI components and consistent look & feel
- Router guards that use `state` to persist the original target route (so the user returns to the intended page after login)

## Auth client SDK
- Plan and scaffold for an NPM package `@coreaccess/client`:
  - Encapsulate PKCE generation, login redirect, token exchange, token storage and automatic refresh.
  - Provide a simple router guard integration and an HTTP wrapper to attach tokens to requests.

## Initial setup flow
- `GET /admin/config` reports whether setup is complete.
- If not complete, redirect users to a Setup Wizard in the Admin UI that posts to `POST /admin/setup` with the initial settings and admin user details.
- Distinguish between runtime settings (saved in DB, no restart required) and ENV/runtime-start settings (DB connection string, signing key seed) that require a container restart to change.

---

# Development, local-run and Docker

## Development notes
- Always mount a persistent volume for the container (DB file, key files, DataProtection keys).
- For local testing, SQLite saved in the mounted volume works well.

## Example docker-compose snippet
```yaml
version: "3.8"
services:
  coreaccess:
    build: .
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - COREACCESS_DEBUGMODE=true
    ports:
      - "5000:80"
    volumes:
      - coreaccess-data:/var/coreaccess
volumes:
  coreaccess-data:
```

---

# Roadmap (high-level milestones)
1. Stabilize User / Role / Permission domain + unit tests.  
2. Admin UI basic CRUD pages (Users, Roles, Permissions).  
3. Audit & Access log tables + Admin UI views for logs.  
4. Monitoring (health endpoints, metrics) and optional live streaming UI polish.  
5. Key management improvements (rotation UI, integration with Key Vaults, dashboards).  
6. `@coreaccess/client` SDK & optional Vue components for quick integration.

---

# Contributing and how to help
- Implement missing user/role endpoints and the mapping tests.  
- Build the Admin UI CRUD pages and the Setup Wizard UI (multi-step).  
- Implement Audit/Access log tables, endpoints and an Admin UI page to query them.  
- Add unit tests for TokenService and SettingsService.

---

# Important operational notes and gotchas
- Ensure persistent storage for DB and key files — ephemeral containers without mounts will lose critical state.
- Never store or log secrets in plaintext.
- Always persist key material for signing tokens. If OpenIddict starts without a signing key, it will throw and refuse to start.
- Use IMemoryCache (with invalidation) to prevent per-request DB hits for settings used in hot paths (e.g. token issuance).
- Be defensive in mapping code (use `?.Select(...) ?? new List<T>()` when reading navigation collections).

---

# Contact
For design decisions and development context, refer to internal project notes and the active conversation logs in the project chat.

