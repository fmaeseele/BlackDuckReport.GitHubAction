using System;
using System.IO;
using System.Text;

using BlackDuckReport.GitHubAction.Api;
using BlackDuckReport.GitHubAction.Service;

namespace BlackDuckReport.GitHubAction;

public class ActionOutputs
{
    private const string propertyName = "blackduck-scan-security-report";
    private readonly BlackDuckReportGeneratorService _blackDuckReportGeneratorService;
    private readonly BlackDuckApi.Models.Project _project;

    public ActionOutputs(BlackDuckReportGeneratorService blackDuckReportGeneratorService, BlackDuckApi.Models.Project project)
    {
        ArgumentNullException.ThrowIfNull(blackDuckReportGeneratorService);
        ArgumentNullException.ThrowIfNull(project);

        _blackDuckReportGeneratorService = blackDuckReportGeneratorService;
        _project = project;
    }

    public void BuildOutput()
    {
        ArgumentNullException.ThrowIfNull(_blackDuckReportGeneratorService);
        ArgumentNullException.ThrowIfNull(_project);

        var githubOutputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT", EnvironmentVariableTarget.Process);
        if (!string.IsNullOrWhiteSpace(githubOutputFile))
        {
            // Build Github action output

            // https://docs.github.com/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
            // ::set-output deprecated as mentioned in https://github.blog/changelog/2022-10-11-github-actions-deprecating-save-state-and-set-output-commands/

            var markdownSummary = _blackDuckReportGeneratorService.GenerateMarkdownSecurityReport(_project);

            using var textWriter = new StreamWriter(githubOutputFile, true, Encoding.UTF8);
            textWriter.WriteLine($"{propertyName}<<EOF");
            textWriter.WriteLine(markdownSummary);
            textWriter.WriteLine("EOF");
        }
        else
        {
            // Display Summary onto the Console

            var consoleSummary = _blackDuckReportGeneratorService.GenerateConsoleSecurityReport(_project);

            Console.Write(consoleSummary);
        }
    }
}
