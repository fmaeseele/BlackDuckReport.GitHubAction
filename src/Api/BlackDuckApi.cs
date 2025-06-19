using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BlackDuckReport.GitHubAction.Extensions;
using BlackDuckReport.GitHubAction.Http;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlackDuckReport.GitHubAction.Api;

public sealed partial class BlackDuckApi
{
    private readonly ILogger<BlackDuckApi>? _logger;
    private readonly RestClient _restClient;
    private readonly string _authToken;
    private readonly string _projectName;

    public Json.OAuth2Token? Token { get; private set; }

    public bool IsLogged => Token?.IsLogged ?? false;

    public bool IsTokenExpired => Token?.IsTokenExpired ?? false;

    public BlackDuckApi(IServiceProvider serviceProvider, Uri url, string authToken, string projectName)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        ArgumentNullException.ThrowIfNull(url);
        ArgumentException.ThrowIfNullOrEmpty(authToken);
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        _logger = serviceProvider.GetService<ILogger<BlackDuckApi>>();

        _authToken = authToken;
        _projectName = projectName;
        var configuration = new RestConfiguration()
        {
            BaseAddress = url,
            UseFiddlerProxy = true
        };
        _restClient = new RestClient(serviceProvider, configuration);
    }

    /// <summary>
    /// Authenticate with the BlackDuck site
    /// 
    /// https://sap.blackducksoftware.com/api-doc/public.html#_authentication
    /// </summary>
    /// <returns></returns>
    public async Task LoginAsync(CancellationToken cancellationToken = default)
    {
        using var loggerScope = _logger?.BeginScope("LoginAsync");

        var request = RestClient.CreatePostHttpMessage(
            queryUri: Constants.HTTP_LOGIN_WITH_TOKEN_URL
        )
        .AcceptContent(Constants.HTTP_ACCEPT_JSON)
        .WithAuthorization(Constants.HTTP_AUTHORIZATION_TOKEN + _authToken);

        Token = await _restClient.SendJsonMessageAsync(request, OAuth2TokenContext.Default.OAuth2Token, ErrorContext.Default.Error, false, cancellationToken: cancellationToken).ConfigureAwait(false);

        _logger?.LogDebug("Authentication Successful");
    }

    public async Task<List<Models.Project>> GetDashboardAsync(Version? projectVersion, CancellationToken cancellationToken = default)
    {
        using var loggerScope = _logger?.BeginScope("GetDashboardAsync");

        if (!IsLogged)
            throw new InvalidOperationException();

        _logger?.LogInformation("Requesting project information for {projectName}", _projectName);

        var request = RestClient.CreateGetHttpMessage(Constants.HTTP_PROJECT_VERSIONS_URL)
        .WithQueryParameters(new KeyValueList<string, string>
        {
            { "limit", "100" },
            { "offset", "0" },
            { "q", _projectName },
        })
        .WithBearerToken(Token?.BearerToken);

        var modelProjects = new List<Models.Project>();

        var projects = await _restClient.SendJsonMessageAsync(request, ProjectVersionsContext.Default.ProjectVersions, ErrorContext.Default.Error, false, cancellationToken: cancellationToken).ConfigureAwait(false);

        if (projects.Items is not null)
            foreach (var project in projects.Items)
            {
                if (project is null)
                    continue;
                //if (projectVersion is not null && project.VersionName != projectVersion.ToString())
                //    continue;
                var url = project.Meta?.Href?.ToString() + "/vulnerability-bom";
                var request2 = RestClient.CreateGetHttpMessage(url)
                .WithQueryParameters(new KeyValueList<string, string>
                {
                    { "limit", "100" },
                    { "offset", "0" },
                })
                .WithBearerToken(Token?.BearerToken);

                _logger?.LogInformation("Requesting project components for {projectName} with version: {version}", _projectName, project.VersionName);

                var result = await _restClient.SendJsonMessageAsync(request2, ProjectVulnerabilitiesContext.Default.ProjectVulnerabilities, ErrorContext.Default.Error, false, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (result.Items is null)
                    continue;

                modelProjects.Add(new Models.Project(project, result.Items));
            }

        return modelProjects;
    }
}
