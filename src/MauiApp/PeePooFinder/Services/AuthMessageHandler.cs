using System.Net;
using System.Net.Http.Headers;

namespace PeePooFinder.Services;

/// <summary>Attaches the bearer token, and ends the local session when the server rejects it.</summary>
public class AuthMessageHandler : DelegatingHandler
{
    private readonly ISessionService _session;

    public AuthMessageHandler(ISessionService session)
    {
        _session = session;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _session.GetTokenAsync();
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        var isSignIn = request.RequestUri?.AbsolutePath.EndsWith("/account/login", StringComparison.OrdinalIgnoreCase) ?? false;
        if (response.StatusCode == HttpStatusCode.Unauthorized && !string.IsNullOrEmpty(token) && !isSignIn)
        {
            await _session.ClearAsync();
            _session.NotifyExpired();
        }

        return response;
    }
}
