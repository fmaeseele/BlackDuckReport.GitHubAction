using System.Net.Http.Headers;

namespace BlackDuckReport.GitHubAction.Http;

public class BearerAuthenticationHeaderValue(string token)
    : AuthenticationHeaderValue(AuthenticationScheme, token)
{
    private const string AuthenticationScheme = "Bearer";
}
