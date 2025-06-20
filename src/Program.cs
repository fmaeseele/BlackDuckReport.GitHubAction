using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using BlackDuckReport.GitHubAction.Service;

using CommandLine;

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

    static async Task StartAnalysisAsync(ActionInputs inputs, IHost host)
    {
        ArgumentNullException.ThrowIfNull(inputs);
        ArgumentNullException.ThrowIfNull(host);

        using CancellationTokenSource tokenSource = new();

        Console.CancelKeyPress += delegate
        {
            tokenSource.Cancel();
        };

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
            tokenSource.Token).ConfigureAwait(false);
        if (!projectList.Any())
            throw new InvalidOperationException($"Project not found: {inputs.ProjectName}");
        // Filter on specific project name, if provided
        var project = projectList.FirstOrDefault(p => p.Name is not null && p.Name.Equals(inputs.ProjectName, StringComparison.OrdinalIgnoreCase))
            ?? throw new InvalidOperationException($"Project not found: {inputs.ProjectName}");

        // Build the output
        var githubActionOutput = new ActionOutputs(blackDuckReportGeneratorService, project);
        githubActionOutput.BuildOutput();
    }

    static async Task Main(string[] args)
    {
        using IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((_, services) => services.AddSingleton<BlackDuckReportGeneratorService>())
            .Build();

        var parser = Parser.Default.ParseArguments<ActionInputs>(() => new(), args);

        parser.WithNotParsed(errors =>
        {
            var logger = Get<ILoggerFactory>(host).CreateLogger(nameof(Program));
            logger.LogError("{error}", string.Join(Environment.NewLine, errors.Select(error => error.ToString())));

            Environment.Exit(2);
        });

        await parser.WithParsedAsync(async (options) =>
        {
            try
            {
                await StartAnalysisAsync(options, host).ConfigureAwait(false);

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                var logger = Get<ILoggerFactory>(host).CreateLogger(nameof(Program));
                logger.LogError(ex, "An error occurred while processing the action inputs.");

                Environment.Exit(1);
            }
        });

        await host.RunAsync();
    }
}