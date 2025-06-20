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

    public void GenerateConsoleSecurityReport(BlackDuckApi.Models.Project project)
    {
        using var scope = _logger?.BeginScope(nameof(GenerateConsoleSecurityReport));

        ArgumentNullException.ThrowIfNull(project);

        Console.WriteLine($"Project: {project.Name} Version: {project.Version} LastUpdatedAt: {project.LastUpdatedAt}");
        Console.WriteLine($"\tVulnerabilities:");
        Console.WriteLine($"\t\tCritical: {project.Vulnerabilities.Critical}");
        Console.WriteLine($"\t\tHigh: {project.Vulnerabilities.High}");
        Console.WriteLine($"\t\tMedium: {project.Vulnerabilities.Medium}");
        Console.WriteLine($"\t\tLow: {project.Vulnerabilities.Low}");
        Console.WriteLine();

        if (project.Vulnerabilities.Critical > 0)
        {
            Console.WriteLine($"\t\tCritical: {project.Vulnerabilities.Critical}");
            foreach (var component in project.ComponentsWithCritical)
                Console.WriteLine($"\t\t  Component: [{component.Name}] Id: [{component.Id}] Count={component.Vulnerabilities.Critical}");
        }

        if (project.Vulnerabilities.High > 0)
        {
            Console.WriteLine($"\t\tHigh: {project.Vulnerabilities.High}");
            foreach (var component in project.ComponentsWithHigh)
                Console.WriteLine($"\t\t  Component: [{component.Name}] Id: [{component.Id}] Count={component.Vulnerabilities.High}");
        }

        if (project.Vulnerabilities.Medium > 0)
        {
            Console.WriteLine($"\t\tMedium: {project.Vulnerabilities.Medium}");
            foreach (var component in project.ComponentsWithMedium)
                Console.WriteLine($"\t\t  Component: [{component.Name}] Id: [{component.Id}] Count={component.Vulnerabilities.Medium}");
        }

        if (project.Vulnerabilities.Low > 0)
        {
            Console.WriteLine($"\t\tLow: {project.Vulnerabilities.Low}");
            foreach (var component in project.ComponentsWithLow)
                Console.WriteLine($"\t\t  Component: [{component.Name}] Id: [{component.Id}] Count={component.Vulnerabilities.Low}");
        }
    }

    public string GenerateMarkdownSecurityReport(BlackDuckApi.Models.Project project)
    {
        using var scope = _logger?.BeginScope(nameof(GenerateMarkdownSecurityReport));

        ArgumentNullException.ThrowIfNull(project);

        var document = new MarkdownDocument();

        document.Append(new MarkdownParagraph(
            new MarkdownImage("BlackDuck Logo", "https://www.blackduck.com/content/dam/black-duck/en-us/images/BlackDuckLogo-OnDark.svg")));

        document.Append(new MarkdownHeader("BlackDuck Scan Security Report", 1));

        document.Append(new MarkdownHeader("Project:", 3));
        document.Append(
            new MarkdownTable(
                new MarkdownTableHeader(
                [
                        new MarkdownTableHeaderCell("Name", MarkdownTableTextAlignment.Left),
                        new MarkdownTableHeaderCell("Version", MarkdownTableTextAlignment.Left),
                        new MarkdownTableHeaderCell("Last Updated", MarkdownTableTextAlignment.Left)
                ]),
                [
                    new MarkdownTableRow(
                        project.Name ?? "Unknown",
                        project.Version ?? "Unknown",
                        project.LastUpdatedAt.ToString() ?? "Unknown"),
                ]
            ));

        document.Append(new MarkdownHeader("Security vulnerabilities Summary:", 3));
        document.Append(
            new MarkdownTable(
                new MarkdownTableHeader(
                [
                        new MarkdownTableHeaderCell("Critical", MarkdownTableTextAlignment.Center),
                        new MarkdownTableHeaderCell("High", MarkdownTableTextAlignment.Center),
                        new MarkdownTableHeaderCell("Medium", MarkdownTableTextAlignment.Center),
                        new MarkdownTableHeaderCell("Low", MarkdownTableTextAlignment.Center),
                ]),
                [
                    new MarkdownTableRow(
                        $"{project.Vulnerabilities.Critical}",
                        $"{project.Vulnerabilities.High}",
                        $"{project.Vulnerabilities.Medium}",
                        $"{project.Vulnerabilities.Low}"
                    )
                ]
            ));

        document.Append(new MarkdownHeader("Security vulnerabilities Details:", 3));

        if (project.Vulnerabilities.Critical > 0)
        {
            document.Append(new MarkdownHeader($"Critical: {project.Vulnerabilities.Critical}", 4));
            document.Append(
                new MarkdownTable(
                    new MarkdownTableHeader(
                    [
                        new MarkdownTableHeaderCell("Component", MarkdownTableTextAlignment.Left),
                    new MarkdownTableHeaderCell("Id", MarkdownTableTextAlignment.Left),
                    new MarkdownTableHeaderCell("Count", MarkdownTableTextAlignment.Center)
                    ]),
                    [.. project.ComponentsWithCritical.Select(c => new MarkdownTableRow(
                    c.Name ?? "Unknown",
                    c.Id ?? "Unknown",
                    $"{c.Vulnerabilities.Critical}"
                ))]
            ));
        }

        if (project.Vulnerabilities.High > 0)
        {
            document.Append(new MarkdownHeader($"High: {project.Vulnerabilities.High}", 4));
            document.Append(
                new MarkdownTable(
                    new MarkdownTableHeader(
                    [
                        new MarkdownTableHeaderCell("Component", MarkdownTableTextAlignment.Left),
                    new MarkdownTableHeaderCell("Id", MarkdownTableTextAlignment.Left),
                    new MarkdownTableHeaderCell("Count", MarkdownTableTextAlignment.Center)
                    ]),
                    [.. project.ComponentsWithHigh.Select(c => new MarkdownTableRow(
                    c.Name ?? "Unknown",
                    c.Id ?? "Unknown",
                    $"{c.Vulnerabilities.High}"
                ))]
            ));
        }

        if (project.Vulnerabilities.Medium > 0)
        {
            document.Append(new MarkdownHeader($"Medium: {project.Vulnerabilities.Medium}", 4));
            document.Append(
                new MarkdownTable(
                    new MarkdownTableHeader(
                    [
                        new MarkdownTableHeaderCell("Component", MarkdownTableTextAlignment.Left),
                    new MarkdownTableHeaderCell("Id", MarkdownTableTextAlignment.Left),
                    new MarkdownTableHeaderCell("Count", MarkdownTableTextAlignment.Center)
                    ]),
                    [.. project.ComponentsWithMedium.Select(c => new MarkdownTableRow(
                    c.Name ?? "Unknown",
                    c.Id ?? "Unknown",
                    $"{c.Vulnerabilities.Medium}"
                ))]
            ));
        }

        if (project.Vulnerabilities.Low > 0)
        {
            document.Append(new MarkdownHeader($"Medium: {project.Vulnerabilities.Medium}", 4));
            document.Append(
                new MarkdownTable(
                    new MarkdownTableHeader(
                    [
                        new MarkdownTableHeaderCell("Component", MarkdownTableTextAlignment.Left),
                    new MarkdownTableHeaderCell("Id", MarkdownTableTextAlignment.Left),
                    new MarkdownTableHeaderCell("Count", MarkdownTableTextAlignment.Center)
                    ]),
                    [.. project.ComponentsWithMedium.Select(c => new MarkdownTableRow(
                    c.Name ?? "Unknown",
                    c.Id ?? "Unknown",
                    $"{c.Vulnerabilities.Medium}"
                ))]
            ));
        }

        return document.ToString();
    }
}
