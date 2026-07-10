using System.Net.Http.Headers;

namespace PeePooFinder.Services;

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

        return await base.SendAsync(request, cancellationToken);
    }
}
