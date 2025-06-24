using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using BlackDuckReport.GitHubAction.Api;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlackDuckReport.GitHubAction.Service;

/// <summary>
/// Provides services for querying the Black Duck API and generating security reports for projects.
/// </summary>
/// <remarks>This service facilitates interaction with the Black Duck API to retrieve project information and
/// generate detailed security reports in various formats, such as console output and Markdown. It requires an
/// <IServiceProvider> to resolve dependencies, including logging and API access.</remarks>
public sealed class BlackDuckReportGeneratorService
{
    private readonly ILogger? _logger;
    private readonly IServiceProvider _serviceProvider;

    public BlackDuckReportGeneratorService(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILogger<BlackDuckReportGeneratorService>>();
    }

    /// <summary>
    /// Queries the Black Duck API for projects matching the specified criteria.
    /// </summary>
    /// <remarks>This method authenticates with the Black Duck API using the provided <paramref
    /// name="authToken"/> and queries for projects that match the specified <paramref name="projectName"/> and
    /// optionally the <paramref name="projectVersion"/>. Ensure that the <paramref name="authToken"/> has sufficient
    /// permissions to access the API.</remarks>
    /// <param name="url">The base URL of the Black Duck API. This parameter cannot be <see langword="null"/>.</param>
    /// <param name="authToken">The authentication token used to access the Black Duck API. This parameter cannot be <see langword="null"/> or
    /// empty.</param>
    /// <param name="projectName">The name of the project to query. This parameter cannot be <see langword="null"/> or empty.</param>
    /// <param name="projectVersion">The specific version of the project to query, or <see langword="null"/> to query all versions.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A read-only list of <see cref="BlackDuckApi.Models.Project"/> objects that match the specified criteria. The
    /// list will be empty if no matching projects are found.</returns>
    public async Task<IReadOnlyList<BlackDuckApi.Models.Project>> BlackDuckQueryProjectAsync(Uri? url, string? authToken, string? projectName, Version? projectVersion, CancellationToken cancellationToken)
    {
        using var scope = _logger?.BeginScope(nameof(BlackDuckQueryProjectAsync));

        ArgumentNullException.ThrowIfNull(url);
        ArgumentException.ThrowIfNullOrEmpty(authToken);
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        _logger?.LogTrace("GenerateBlackDuckReportAsync: Uri={url} AuthToken={authToken} ProjectName={projectName} ProjectVersion={projectVersion}", url, authToken, projectName, projectVersion);

        var api = new BlackDuckApi(_serviceProvider, url, authToken, projectName);
        await api.LoginAsync(cancellationToken).ConfigureAwait(false);
        return await api.GetDashboardAsync(projectVersion, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Generates a detailed security report for the specified project, including vulnerability counts and information
    /// about affected components and dependencies.
    /// </summary>
    /// <remarks>The report provides a breakdown of vulnerabilities by severity (Critical, High, Medium, Low),
    /// along with detailed information about components contributing to each severity level. It also lists the
    /// project's direct dependencies and their associated vulnerabilities.</remarks>
    /// <param name="project">The project for which the security report is generated. This parameter cannot be <see langword="null"/>.</param>
    /// <returns>A formatted string containing the security report, which includes the project's name, version, last updated
    /// timestamp, vulnerability counts (grouped by severity), and details about components and direct dependencies.</returns>
    public string GenerateConsoleSecurityReport(BlackDuckApi.Models.Project project)
    {
        using var scope = _logger?.BeginScope(nameof(GenerateConsoleSecurityReport));

        ArgumentNullException.ThrowIfNull(project);

        var sb = new StringBuilder();
        sb.AppendLine($"Project: {project.Name} Version: {project.Version} LastUpdatedAt: {project.LastUpdatedAt}");
        sb.AppendLine($"\tVulnerabilities:");
        sb.AppendLine($"\t\tCritical: {project.Vulnerabilities.Critical}");
        sb.AppendLine($"\t\tHigh: {project.Vulnerabilities.High}");
        sb.AppendLine($"\t\tMedium: {project.Vulnerabilities.Medium}");
        sb.AppendLine($"\t\tLow: {project.Vulnerabilities.Low}");
        sb.AppendLine();

        if (project.Vulnerabilities.Critical > 0)
        {
            sb.AppendLine($"\t\tCritical: {project.Vulnerabilities.Critical}");
            foreach (var component in project.ComponentsWithCritical)
                sb.AppendLine($"\t\t  Component: [{component.Name}] Version: [{component.Version}] Count={component.Vulnerabilities.Critical}");
        }

        if (project.Vulnerabilities.High > 0)
        {
            sb.AppendLine($"\t\tHigh: {project.Vulnerabilities.High}");
            foreach (var component in project.ComponentsWithHigh)
                sb.AppendLine($"\t\t  Component: [{component.Name}] Version: [{component.Version}] Count={component.Vulnerabilities.High}");
        }

        if (project.Vulnerabilities.Medium > 0)
        {
            sb.AppendLine($"\t\tMedium: {project.Vulnerabilities.Medium}");
            foreach (var component in project.ComponentsWithMedium)
                sb.AppendLine($"\t\t  Component: [{component.Name}] Version: [{component.Version}] Count={component.Vulnerabilities.Medium}");
        }

        if (project.Vulnerabilities.Low > 0)
        {
            sb.AppendLine($"\t\tLow: {project.Vulnerabilities.Low}");
            foreach (var component in project.ComponentsWithLow)
                sb.AppendLine($"\t\t  Component: [{component.Name}] Version: [{component.Version}] Count={component.Vulnerabilities.Low}");
        }

        sb.AppendLine();
        sb.AppendLine($"\tDirect Dependencies:");
        foreach (var component in project.DirectDependencies.OrderBy(c => c.Id))
            sb.AppendLine($"\t\t  Component: [{component.Name}] Version: [{component.Version}] Count={component.Vulnerabilities.Count}");

        return sb.ToString();
    }

    /// <summary>
    /// Generates a Markdown-formatted security report for the specified project.
    /// </summary>
    /// <remarks>The generated report includes the following sections: <list type="bullet">
    /// <item><description>Project details, such as name, version, and last updated timestamp.</description></item>
    /// <item><description>A summary of security vulnerabilities categorized as Critical, High, Medium, and
    /// Low.</description></item> <item><description>Detailed tables listing components with vulnerabilities for each
    /// severity level, if applicable.</description></item> </list> The report is formatted in Markdown and can be used
    /// in documentation, dashboards, or other Markdown-compatible tools.</remarks>
    /// <param name="project">The project for which the security report is generated. This parameter cannot be <see langword="null"/>.</param>
    /// <returns>A string containing the Markdown-formatted security report, including project details, a summary of  security
    /// vulnerabilities, and detailed information about vulnerabilities categorized by severity.</returns>
    public string GenerateMarkdownSecurityReport(BlackDuckApi.Models.Project project)
    {
        using var scope = _logger?.BeginScope(nameof(GenerateMarkdownSecurityReport));

        ArgumentNullException.ThrowIfNull(project);

        // Emoji list for Markdown:
        // https://gist.github.com/rxaviers/7360908#file-gistfile1-md


        var sb = new StringBuilder();

        sb.AppendLine("<img src=\"https://www.blackduck.com/content/dam/black-duck/en-us/images/BlackDuckLogo-OnDark.svg\" alt=\"BlackDuck Logo\" height=\"50\" />");
        sb.AppendLine();
        sb.AppendLine("# BlackDuck Scan Security Report");
        sb.AppendLine();
        sb.AppendLine("### Project:");
        sb.AppendLine();
        sb.AppendLine("| Name | Version | Last Updated |");
        sb.AppendLine("| :--- | :------ | :----------- |");
        sb.AppendLine($"| `{project.Name ?? "Unknown"}` | `{project.Version ?? "Unknown"}` | `{project.LastUpdatedAt.ToString() ?? "Unknown"}` |");
        sb.AppendLine();
        sb.AppendLine("### Security vulnerabilities Summary:");
        sb.AppendLine("| Critical | High | Medium | Low |");
        sb.AppendLine("| :------: | :--: | :----: | :-: |");
        sb.AppendLine($"|{CriticalEmoji}{project.Vulnerabilities.Critical} | {HighEmoji}{project.Vulnerabilities.High} | {MediumEmoji}{project.Vulnerabilities.Medium} | {project.Vulnerabilities.Low} |");
        sb.AppendLine();
        sb.AppendLine("### Security vulnerabilities Details:");

        if (project.Vulnerabilities.Critical > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"#### Critical: {CriticalEmoji}`{project.Vulnerabilities.Critical}`");
            sb.AppendLine();
            sb.AppendLine("| Component | Version | Count |");
            sb.AppendLine("| :-------- | :------ | :---: |");
            foreach (var component in project.ComponentsWithCritical)
            {
                sb.AppendLine($"| {component.Name ?? "Unknown"} | {component.Version ?? "Unknown"} | {component.Vulnerabilities.Critical} |");
            }
        }

        if (project.Vulnerabilities.High > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"#### High: {HighEmoji}`{project.Vulnerabilities.High}`");
            sb.AppendLine();
            sb.AppendLine("| Component | Version | Count |");
            sb.AppendLine("| :-------- | :------ | :---: |");
            foreach (var component in project.ComponentsWithHigh)
            {
                sb.AppendLine($"| {component.Name ?? "Unknown"} | {component.Version ?? "Unknown"} | {component.Vulnerabilities.High} |");
            }
        }

        if (project.Vulnerabilities.Medium > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"#### Medium: {MediumEmoji}{project.Vulnerabilities.Medium}");
            sb.AppendLine();
            sb.AppendLine("| Component | Version | Count |");
            sb.AppendLine("| :-------- | :------ | :---: |");
            foreach (var component in project.ComponentsWithMedium)
            {
                sb.AppendLine($"| {component.Name ?? "Unknown"} | {component.Version ?? "Unknown"} | {component.Vulnerabilities.Medium} |");
            }
        }

        if (project.Vulnerabilities.Low > 0)
        {
            sb.AppendLine();
            sb.AppendLine($"#### Low: {project.Vulnerabilities.Low}");
            sb.AppendLine();
            sb.AppendLine("| Component | Version | Count |");
            sb.AppendLine("| :-------- | :------ | :---: |");
            foreach (var component in project.ComponentsWithLow)
            {
                sb.AppendLine($"| {component.Name ?? "Unknown"} | {component.Version ?? "Unknown"} | {component.Vulnerabilities.Low} |");
            }
        }

        sb.AppendLine();
        sb.AppendLine("### Direct Dependencies:");
        sb.AppendLine();
        sb.AppendLine("| Component | Version | Status |");
        sb.AppendLine("| :-------- | :------ | :----: |");
        foreach (var component in project.DirectDependencies)
        {
            var status = component.Vulnerabilities.Count == 0 ? OkEmoji : CriticalEmoji;
            sb.AppendLine($"| {component.Name ?? "Unknown"} | {component.Version ?? "Unknown"} | {status} |");
        }

        return sb.ToString();
    }

    private static string CriticalEmoji => ":x:";
    private static string HighEmoji => ":red_circle:";
    private static string MediumEmoji => ":large_orange_diamond:";
    private static string OkEmoji => ":white_check_mark:";
}
