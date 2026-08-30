# EntraSamlLab

EntraSamlLab is a .NET 8 Blazor Web App that provides a focused workspace for
building and validating a Microsoft Entra ID SAML 2.0 Service Provider
integration. This first version is the base application only: SAML middleware
and Microsoft Entra authentication are intentionally not configured yet.

## Application purpose

The dashboard provides:

- An overview of authentication and service provider readiness
- A SAML status page for session and connection details
- A ClaimsPrincipal claims inspector
- A configuration page for public, non-secret SAML values
- A plain-text health endpoint at `/health`

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

## Future SAML configuration

Populate the non-secret values in the `Saml` section of `appsettings.json` or
an environment-specific configuration source:

- Application name
- Public base URL
- Service provider Entity ID
- Assertion Consumer Service URL
- SAML metadata URL
- Identity provider Entity ID
- Identity provider login URL

Do not commit passwords, certificates, private keys, tenant secrets, or other
credentials. Sustainsys.Saml2 is not installed in this baseline and will be
added only after the base application has been validated.