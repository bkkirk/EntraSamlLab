# EntraSamlLab

EntraSamlLab is a .NET 8 Blazor Web App that provides a focused workspace for
building and validating a Microsoft Entra ID SAML 2.0 Service Provider
integration. It uses Sustainsys.Saml2 for the SAML Service Provider protocol
flow and cookie authentication for the local application session.

## Application purpose

The dashboard provides:

- An overview of authentication and service provider readiness
- A SAML status page for session and connection details
- A ClaimsPrincipal claims inspector
- A configuration page for public, non-secret SAML values
- A plain-text health endpoint at `/health`
- SAML metadata at `/Saml2`
- Guarded login at `/auth/login` and local logout at `/auth/logout`

The application uses interactive server rendering and reads SAML settings from
the `Saml` section in `appsettings.json`.

## Local execution

The project targets .NET 8. Confirm the SDK is available, then restore and
build:

```bash
dotnet restore
dotnet build
```

Run the application with:

```bash
dotnet run --no-launch-profile --project EntraSamlLab.csproj
```

The app listens on `0.0.0.0` and uses the `PORT` environment variable when it
is present. It defaults to port `8080`:

```bash
PORT=8088 dotnet run --no-launch-profile --project EntraSamlLab.csproj
```

Open the root URL to use the dashboard. The application also processes
`X-Forwarded-For` and `X-Forwarded-Proto` headers so a reverse proxy can
represent the public HTTPS connection correctly.

## Docker build

Build the multi-stage image from the project root:

```bash
docker build -t entrasamllab .
```

The build uses `mcr.microsoft.com/dotnet/sdk:8.0` and publishes the app in
Release mode. The runtime image uses
`mcr.microsoft.com/dotnet/aspnet:8.0`.

## Docker run

The container exposes port `8080` and sets `ASPNETCORE_URLS=http://+:8080`:

```bash
docker run --rm -p 8080:8080 entrasamllab
```

## Health endpoint

The health endpoint returns HTTP 200 and plain text:

```bash
curl http://localhost:8080/health
```

Expected response:

```text
EntraSamlLab Healthy
```

## SAML Service Provider Configuration

The current Service Provider is configured with these public values:

- **Public URL:** `https://www.bkkirk.com`
- **Entity ID:** `https://www.bkkirk.com/Saml2`
- **ACS URL:** `https://www.bkkirk.com/Saml2/Acs`
- **Metadata URL:** `https://www.bkkirk.com/Saml2`

The Service Provider metadata endpoint is available at `/Saml2`. Microsoft
Entra ID is configured as the Identity Provider using the application-specific
federation metadata URL and the tenant's SAML endpoints. Sustainsys uses the
federation metadata to discover and validate the current Entra signing
certificate; no downloaded certificate is embedded in the application.

The configured Identity Provider values are:

- **Entity ID / issuer:** `https://sts.windows.net/0561ec8f-2290-4f1f-8433-04350e745711/`
- **Login URL:** `https://login.microsoftonline.com/0561ec8f-2290-4f1f-8433-04350e745711/saml2`
- **Logout URL:** `https://login.microsoftonline.com/0561ec8f-2290-4f1f-8433-04350e745711/saml2`
- **Federation metadata URL:** `https://login.microsoftonline.com/0561ec8f-2290-4f1f-8433-04350e745711/federationmetadata/2007-06/federationmetadata.xml?appid=11e77e71-d253-42a7-a265-cca453fe7623`

The `/auth/login` endpoint starts a SAML challenge when the IdP configuration
is complete. Successful responses establish the local cookie session and
return the user to the dashboard. Remote failures are logged with diagnostic
context and redirect to `/saml-status?message=saml-authentication-failed`
without displaying raw SAML responses or tokens. `/auth/logout` clears the
local cookie session and returns to the dashboard.

## Configuration values

Populate the non-secret values in the `Saml` section of `appsettings.json` or
an environment-specific configuration source:

- Application name
- Public base URL
- Service provider Entity ID
- Assertion Consumer Service URL
- SAML metadata URL
- Identity provider Entity ID
- Identity provider login URL
- Identity provider logout URL
- Identity provider metadata URL

Do not commit passwords, certificates, private keys, tenant secrets, or other
credentials. The application never displays secret or certificate material in
the Configuration page. Signature, audience, and certificate validation remain
enabled by default.

## Authentication diagnostics

If Entra authentication fails, inspect the application logs for the SAML
failure record. The diagnostic message points toward the common integration
checks: Entity ID and reply URL matching, audience validation, federation
metadata and signing-certificate validation, NameID presence, and SAML
response validity. Interactive sign-in must be validated from the deployed
public HTTPS URL; it is not performed from the Replit environment.