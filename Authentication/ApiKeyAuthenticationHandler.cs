
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace Demo1.Authentication;
public class ApiKeyAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IConfiguration _configuration;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IConfiguration configuration)
        : base(options, logger, encoder)
    {
        _configuration = configuration;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var headerName =
            _configuration["ApiKey:HeaderName"] ?? "X-API-KEY";

        var configuredApiKey =
            _configuration["ApiKey:Key"];

        if (!Request.Headers.TryGetValue(headerName, out var apiKey))
        {
            return Task.FromResult(
                AuthenticateResult.Fail("API Key tidak dijumpai."));
        }

        if (apiKey != configuredApiKey)
        {
            return Task.FromResult(
                AuthenticateResult.Fail("API Key tidak sah."));
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, "TrustedClient"),
            new Claim("client_type", "api-key")
        };

        var identity = new ClaimsIdentity(
            claims,
            Scheme.Name);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            Scheme.Name);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
