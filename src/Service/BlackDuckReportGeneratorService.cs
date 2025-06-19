using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BlackDuckReport.GitHubAction.Api;

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

    public async Task GenerateBlackDuckReportAsync(Uri? url, string? authToken, string? projectName, Version? projectVersion, CancellationToken cancellationToken)
    {
        using var scope = _logger?.BeginScope("GenerateBlackDuckReportAsync");

        ArgumentNullException.ThrowIfNull(url);
        ArgumentException.ThrowIfNullOrEmpty(authToken);
        ArgumentException.ThrowIfNullOrEmpty(projectName);

        _logger?.LogInformation("GenerateBlackDuckReportAsync: Uri={url} AuthToken={authToken} ProjectName={projectName} ProjectVersion={projectVersion}", url, authToken, projectName, projectVersion);

        var api = new BlackDuckApi(_serviceProvider, url, authToken, projectName);
        await api.LoginAsync(cancellationToken).ConfigureAwait(false);
        var projects = await api.GetDashboardAsync(projectVersion, cancellationToken).ConfigureAwait(false);
        DumpReport(projectName, projectVersion, projects);
    }

    private static void DumpReport(string projectName, Version? projectVersion, List<BlackDuckApi.Models.Project> projects)
    {
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
}
