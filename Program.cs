using System.Net;
using EntraSamlLab;
using EntraSamlLab.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;
using Sustainsys.Saml2;
using Sustainsys.Saml2.AspNetCore2;
using Sustainsys.Saml2.Metadata;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.Configure<SamlOptions>(
    builder.Configuration.GetSection(SamlOptions.SectionName));

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = Saml2Defaults.Scheme;
    })
    .AddCookie(options =>
    {
        options.Cookie.Name = "EntraSamlLab.Auth";
        options.LoginPath = "/auth/login";
        options.LogoutPath = "/auth/logout";
        options.SlidingExpiration = true;
    })
    .AddSaml2(options =>
    {
        var saml = builder.Configuration
            .GetSection(SamlOptions.SectionName)
            .Get<SamlOptions>() ?? new SamlOptions();

        if (Uri.TryCreate(saml.EntityId, UriKind.Absolute, out var entityId))
        {
            options.SPOptions.EntityId = new EntityId(entityId.AbsoluteUri);
        }

        if (Uri.TryCreate(saml.PublicBaseUrl, UriKind.Absolute, out var publicOrigin))
        {
            options.SPOptions.PublicOrigin = publicOrigin;
        }

        options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.SignOutScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.Events = new RemoteAuthenticationEvents
        {
            OnRemoteFailure = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILoggerFactory>()
                    .CreateLogger("SamlAuthentication");

                logger.LogError(
                    context.Failure,
                    "SAML authentication failed during the remote callback. " +
                    "Check the Entra entity ID, reply URL, audience, federation metadata, " +
                    "signing certificate, NameID, and SAML response.");

                if (!context.Response.HasStarted)
                {
                    context.Response.Redirect("/saml-status?message=saml-authentication-failed");
                    context.HandleResponse();
                }

                return Task.CompletedTask;
            }
        };

        if (saml.HasIdentityProviderConfiguration &&
            Uri.TryCreate(saml.IdentityProviderEntityId, UriKind.Absolute, out var identityProviderEntityId))
        {
            var identityProvider = new IdentityProvider(
                new EntityId(identityProviderEntityId.AbsoluteUri),
                options.SPOptions);

            if (Uri.TryCreate(saml.IdentityProviderLoginUrl, UriKind.Absolute, out var loginUrl))
            {
                identityProvider.SingleSignOnServiceUrl = loginUrl;
            }

            if (Uri.TryCreate(saml.IdentityProviderLogoutUrl, UriKind.Absolute, out var logoutUrl))
            {
                identityProvider.SingleLogoutServiceUrl = logoutUrl;
            }

            if (Uri.TryCreate(saml.IdentityProviderMetadataUrl, UriKind.Absolute, out var metadataUrl))
            {
                identityProvider.MetadataLocation = metadataUrl.AbsoluteUri;
            }

            options.IdentityProviders.Add(identityProvider);
        }
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseForwardedHeaders();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Text("EntraSamlLab Healthy", "text/plain"));

app.MapGet("/auth/login", async (
    HttpContext httpContext,
    IOptions<SamlOptions> samlOptions,
    ILoggerFactory loggerFactory) =>
{
    var saml = samlOptions.Value;
    if (!saml.HasIdentityProviderConfiguration)
    {
        return Results.Redirect("/saml-status?message=identity-provider-not-configured");
    }

    try
    {
        await httpContext.ChallengeAsync(
            Saml2Defaults.Scheme,
            new AuthenticationProperties { RedirectUri = "/" });
    }
    catch (Exception exception) when (exception is InvalidOperationException or UriFormatException)
    {
        loggerFactory.CreateLogger("SamlLogin").LogWarning(
            exception,
            "SAML login was requested but the Identity Provider configuration is incomplete.");
        return Results.Redirect("/saml-status?message=identity-provider-not-configured");
    }
    catch (Exception exception)
    {
        loggerFactory.CreateLogger("SamlLogin").LogError(
            exception,
            "SAML login challenge failed before the remote authentication flow started.");
        return Results.Redirect("/saml-status?message=saml-authentication-failed");
    }

    return Results.Empty;
});

app.MapGet("/auth/logout", async (HttpContext httpContext) =>
{
    await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    return Results.Redirect("/");
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
