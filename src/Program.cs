using System;
using System.IO;
using System.Linq;
using System.Text;
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

        //var projectAnalyzer = Get<ProjectMetricDataAnalyzer>(host);

        //Matcher matcher = new();
        //matcher.AddIncludePatterns(new[] { "**/*.csproj", "**/*.vbproj" });

        //Dictionary<string, CodeAnalysisMetricData> metricData = new(StringComparer.OrdinalIgnoreCase);
        //var projects = matcher.GetResultsInFullPath(inputs.Directory);

        //foreach (var project in projects)
        //{
        //    var metrics =
        //        await projectAnalyzer.AnalyzeAsync(
        //            workspace, project, tokenSource.Token);

        //    foreach (var (path, metric) in metrics)
        //    {
        //        metricData[path] = metric;
        //    }
        //}

        //var updatedMetrics = false;
        //var title = "";
        //StringBuilder summary = new();
        //if (metricData is { Count: > 0 })
        //{
        //    var fileName = "CODE_METRICS.md";
        //    var fullPath = Path.Combine(inputs.Directory, fileName);
        //    var logger = Get<ILoggerFactory>(host).CreateLogger(nameof(StartAnalysisAsync));
        //    var fileExists = File.Exists(fullPath);

        //    logger.LogInformation(
        //        $"{(fileExists ? "Updating" : "Creating")} {fileName} markdown file with latest code metric data.");

        //    summary.AppendLine(
        //        title = $"{(fileExists ? "Updated" : "Created")} {fileName} file, analyzed metrics for {metricData.Count} projects.");

        //    foreach (var (path, _) in metricData)
        //    {
        //        summary.AppendLine($"- *{path}*");
        //    }

        //    var contents = metricData.ToMarkDownBody(inputs);
        //    await File.WriteAllTextAsync(
        //        fullPath,
        //        contents,
        //        tokenSource.Token);

        //    updatedMetrics = true;
        //}
        //else
        //{
        //    summary.Append("No metrics were determined.");
        //}

        ArgumentException.ThrowIfNullOrEmpty(inputs.ProjectName);
        ArgumentNullException.ThrowIfNull(inputs.ProjectVersion);
        ArgumentNullException.ThrowIfNull(inputs.BlackDuckUrl);
        ArgumentException.ThrowIfNullOrEmpty(inputs.BlackDuckToken);

        var service = Get<BlackDuckReportGeneratorService>(host);

        await service.GenerateBlackDuckReportAsync(
            inputs.BlackDuckUrl,
            inputs.BlackDuckToken,
            inputs.ProjectName,
            inputs.ProjectVersion,
            tokenSource.Token).ConfigureAwait(false);


        // https://docs.github.com/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
        // ::set-output deprecated as mentioned in https://github.blog/changelog/2022-10-11-github-actions-deprecating-save-state-and-set-output-commands/
        var githubOutputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT", EnvironmentVariableTarget.Process);
        if (!string.IsNullOrWhiteSpace(githubOutputFile))
        {
            using var textWriter = new StreamWriter(githubOutputFile!, true, Encoding.UTF8);
            //textWriter.WriteLine($"updated-metrics={updatedMetrics}");
            //textWriter.WriteLine($"summary-title={title}");
            textWriter.WriteLine("summary-details<<EOF");
            //textWriter.WriteLine(summary);
            textWriter.WriteLine("EOF");
        }
        else
        {
            //Console.WriteLine($"::set-output name=updated-metrics::{updatedMetrics}");
            //Console.WriteLine($"::set-output name=summary-title::{title}");
            //Console.WriteLine($"::set-output name=summary-details::{summary}");
        }

        await Task.CompletedTask;
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