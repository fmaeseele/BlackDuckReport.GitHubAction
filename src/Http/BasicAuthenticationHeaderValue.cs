using System;
using System.Net.Http.Headers;
using System.Text;

namespace BlackDuckReport.GitHubAction.Http;

public class BasicAuthenticationHeaderValue(string username, string password)
    : AuthenticationHeaderValue(AuthenticationScheme, Convert.ToBase64String(Encoding.ASCII.GetBytes($"{username}:{password}")))
{
    private const string AuthenticationScheme = "Basic";
}
