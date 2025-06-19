using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BlackDuckReport.GitHubAction.Api;

using Markdown;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlackDuckReport.GitHubAction.Service;

internal sealed class BlackDuckReportGeneratorService
{
    private readonly ILogger? _logger;
    private readonly IServiceProvider _serviceProvider;

    public BlackDuckReportGeneratorService(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILogger<BlackDuckReportGeneratorService>>();
    }

    public async Task<IReadOnlyList<BlackDuckApi.Models.Project>> BlackDuckQueryProjectAsync(Uri? url, string? authToken, string? projectName, Version? projectVersion, CancellationToken cancellationToken)
    {
        using var scope = _logger?.BeginScope(nameof(BlackDuckQueryProjectAsync));

        ArgumentNullException.ThrowIfNull(url);
        ArgumentException.ThrowIfNullOrEmpty(authToken);
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        _logger?.LogInformation("GenerateBlackDuckReportAsync: Uri={url} AuthToken={authToken} ProjectName={projectName} ProjectVersion={projectVersion}", url, authToken, projectName, projectVersion);

        var api = new BlackDuckApi(_serviceProvider, url, authToken, projectName);
        await api.LoginAsync(cancellationToken).ConfigureAwait(false);
        return await api.GetDashboardAsync(projectVersion, cancellationToken).ConfigureAwait(false);
    }

    public void DumpReport(string projectName, Version? projectVersion, IReadOnlyList<BlackDuckApi.Models.Project> projects)
    {
        using var scope = _logger?.BeginScope(nameof(DumpReport));

        if (projects.Count == 0)
            throw new InvalidOperationException($"Project not found: {projectName}");

        var filteredProjects = projectVersion is null ? projects : projects.Where(p => p.Version == projectVersion.ToString());
        if (!filteredProjects.Any())
            throw new InvalidOperationException($"Project version not found: {projectVersion}");

        foreach (var project in filteredProjects)
        {
            Console.WriteLine($"Project: {project.Name} Version: {project.Version}");
            Console.WriteLine($"\tVulnerabilities:");
            Console.WriteLine($"\t\tCritical: {project.Vulnerabilities.Critical}");
            Console.WriteLine($"\t\tHigh: {project.Vulnerabilities.High}");
            Console.WriteLine($"\t\tMedium: {project.Vulnerabilities.Medium}");
            Console.WriteLine();

            Console.WriteLine($"\t\tCritical: {project.Vulnerabilities.Critical}");
            foreach (var component in project.ComponentsWithCritical)
                Console.WriteLine($"\t\t  Component: [{component.Name}] Id: [{component.Id}] Count={component.Vulnerabilities.Critical}");

            Console.WriteLine($"\t\tHigh: {project.Vulnerabilities.High}");
            foreach (var component in project.ComponentsWithHigh)
                Console.WriteLine($"\t\t  Component: [{component.Name}] Id: [{component.Id}] Count={component.Vulnerabilities.High}");

            Console.WriteLine($"\t\tMedium: {project.Vulnerabilities.Medium}");
            foreach (var component in project.ComponentsWithMedium)
                Console.WriteLine($"\t\t  Component: [{component.Name}] Id: [{component.Id}] Count={component.Vulnerabilities.Medium}");
        }
    }

    public string GenerateMarkdownSecurityReport(BlackDuckApi.Models.Project project)
    {
        using var scope = _logger?.BeginScope(nameof(GenerateMarkdownSecurityReport));

        ArgumentNullException.ThrowIfNull(project);

        var document = new MarkdownDocument();

        document.Append(new MarkdownHeader($"Project: {project.Name} - Version: {project.Version}", 2));
        document.Append(new MarkdownHeader("Vulnerabilities", 3));

        document.Append(new MarkdownUnorderedList(new[]
        {
                new MarkdownText($"Critical: {project.Vulnerabilities.Critical}"),
                new MarkdownText($"High: {project.Vulnerabilities.High}"),
                new MarkdownText($"Medium: {project.Vulnerabilities.Medium}")
            }));

        document.Append(new MarkdownTitle("Critical Components", 4));
        foreach (var c in project.ComponentsWithCritical)
        {
            document.Append(new MarkdownUnorderedListItem(
                new MarkdownText($"Component: {c.Name} (ID: {c.Id}) - Count: {c.Vulnerabilities.Critical}")
            ));
        }

        document.Append(new MarkdownTitle("High Components", 4));
        foreach (var c in project.ComponentsWithHigh)
        {
            document.Append(new MarkdownUnorderedListItem(
                new MarkdownText($"Component: {c.Name} (ID: {c.Id}) - Count: {c.Vulnerabilities.High}")
            ));
        }

        document.Append(new MarkdownTitle("Medium Components", 4));
        foreach (var c in project.ComponentsWithMedium)
        {
            document.Append(new MarkdownUnorderedListItem(
                new MarkdownText($"Component: {c.Name} (ID: {c.Id}) - Count: {c.Vulnerabilities.Medium}")
            ));
        }

        document.Append(new MarkdownNewLine());

        return document.ToString();
    }
}
