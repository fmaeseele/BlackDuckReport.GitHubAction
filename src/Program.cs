using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BlackDuckReport.GitHubAction.Service;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BlackDuckReport.GitHubAction;

public class Program
{
    static TService Get<TService>(IHost host) where TService : notnull
    {
        ArgumentNullException.ThrowIfNull(host);
        return host.Services.GetRequiredService<TService>();
    }

    static async Task StartAnalysisAsync(ActionInputs inputs, IHost host, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(host);

        ArgumentException.ThrowIfNullOrEmpty(inputs.ProjectName);
        ArgumentNullException.ThrowIfNull(inputs.ProjectVersion);
        ArgumentNullException.ThrowIfNull(inputs.BlackDuckUrl);
        ArgumentException.ThrowIfNullOrEmpty(inputs.BlackDuckToken);

        var blackDuckReportGeneratorService = Get<BlackDuckReportGeneratorService>(host);

        // Generate the BlackDuck 
        var projectList = await blackDuckReportGeneratorService.BlackDuckQueryProjectAsync(
            inputs.BlackDuckUrl,
            inputs.BlackDuckToken,
            inputs.ProjectName,
            inputs.ProjectVersion,
            cancellationToken).ConfigureAwait(false);
        if (!projectList.Any())
            throw new InvalidOperationException($"Project name not found: {inputs.ProjectName}");

        Api.BlackDuckApi.Models.Project? project = null;

        // Filter on specific project version if provided
        string? projectVersionString = inputs.ProjectVersion?.ToString();
        if (!string.IsNullOrEmpty(projectVersionString))
        {
            project = projectList.FirstOrDefault(p => projectVersionString == p.Version)
                ?? throw new InvalidOperationException($"Project version not found: {inputs.ProjectVersion}");
        }
        else
        {
            project = projectList[0];
        }

        // Build the output
        var githubActionOutput = new ActionOutputs(blackDuckReportGeneratorService, project);
        githubActionOutput.BuildOutput();

        await Task.CompletedTask;
    }

    static async Task<int> InvokeAsync(string[] args, CancellationToken cancellationToken)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) => services.AddSingleton<BlackDuckReportGeneratorService>())
            .Build();

        var logger = Get<ILoggerFactory>(host).CreateLogger(nameof(Program));

        try
        {
            var inputs = new ActionInputs();
            if (!inputs.Parse(args, logger))
            {
                return 2;
            }

            await StartAnalysisAsync(inputs, host, cancellationToken).ConfigureAwait(false);
            return 0;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while processing the action inputs.");
            return 1;
        }
    }

    static async Task Main(string[] args)
    {
        try
        {
            // Ensure UTF-8 encoding for console output
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            using CancellationTokenSource cancellationTokenSource = new();

            // Handle cancellation gracefully
            Console.CancelKeyPress += delegate { cancellationTokenSource.Cancel(); };

            int exitCode = await InvokeAsync(args, cancellationTokenSource.Token).ConfigureAwait(false);
            Environment.Exit(exitCode);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("An error occurred while running the host: " + ex.Message);
            Console.Error.WriteLine("StackTrace: " + ex.StackTrace);

            Environment.Exit(1);
        }
    }
}