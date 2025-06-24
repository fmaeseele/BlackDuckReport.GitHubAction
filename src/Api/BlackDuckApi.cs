using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using BlackDuckReport.GitHubAction.Extensions;
using BlackDuckReport.GitHubAction.Http;
using BlackDuckReport.GitHubAction.Utils;

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
            UseFiddlerProxy = ProxyHelper.FiddlerRunning
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

    public async Task<IReadOnlyList<Models.Project>> GetDashboardAsync(Version? projectVersion, CancellationToken cancellationToken = default)
    {
        using var loggerScope = _logger?.BeginScope("GetDashboardAsync");

        if (!IsLogged)
            throw new InvalidOperationException();

        _logger?.LogInformation("Requesting project versions for {projectName}", _projectName);

        var request = RestClient.CreateGetHttpMessage(Constants.HTTP_SEARCH_PROJECT_VERSIONS_URL)
        .AcceptContent(Constants.HTTP_ACCEPT_JSON)
        .WithQueryParameters(new KeyValueList<string, string>
        {
            { "limit", "100" },
            { "offset", "0" },
            { "q", _projectName },
        })
        .WithBearerToken(Token?.BearerToken);

        var modelProjects = new List<Models.Project>();

        var queryResult = await _restClient.SendJsonMessageAsync(request, ProjectListQueryResultContext.Default.ProjectListQueryResult, ErrorContext.Default.Error, false, cancellationToken: cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(queryResult);
        if (queryResult.Items is not null)
            foreach (var project in queryResult.Items)
            {
                if (project is null)
                    continue;

                var components = await GetProjectComponents(project, cancellationToken).ConfigureAwait(false);
                modelProjects.Add(new Models.Project(project, components));
            }

        return modelProjects;
    }

    private async Task<IReadOnlyList<Json.ComponentItem>> GetProjectComponents(Json.ProjectItem project, CancellationToken cancellationToken = default)
    {
        using var loggerScope = _logger?.BeginScope(nameof(GetProjectComponents));

        ArgumentNullException.ThrowIfNull(project);

        if (!IsLogged)
            throw new InvalidOperationException();

        _logger?.LogInformation("Requesting project components for {projectName} with version: {version}", project.ProjectName, project.VersionName);

        var url = project.Meta?.Href?.ToString() + "/components";
        var request = RestClient.CreateGetHttpMessage(url)
        .AcceptContent(Constants.HTTP_ACCEPT_JSON)
        .WithQueryParameters(new KeyValueList<string, string>
        {
            { "filter", "bomInclusion:false" },
            { "filter", "bomMatchInclusion:false" },
            { "filter", "bomMatchReviewStatus:reviewed" },
            { "limit", "200" },
            { "offset", "0" },
        })
        .WithBearerToken(Token?.BearerToken);
        var result = await _restClient.SendJsonMessageAsync(request, ComponentListQueryResultContext.Default.ComponentListQueryResult, ErrorContext.Default.Error, false, cancellationToken: cancellationToken).ConfigureAwait(false);
        return result.Items ?? [];
    }
}
