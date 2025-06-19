using System;
using System.IO;
using System.Text;

namespace BlackDuckReport.GitHubAction;

public class ActionOutputs
{
    private const string propertyName = "blackduck-scan-security-report";

    public string? Summary { get; set; }

    public void BuildOutput()
    {
        // https://docs.github.com/actions/reference/workflow-commands-for-github-actions#setting-an-output-parameter
        // ::set-output deprecated as mentioned in https://github.blog/changelog/2022-10-11-github-actions-deprecating-save-state-and-set-output-commands/
        var githubOutputFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT", EnvironmentVariableTarget.Process);
        if (!string.IsNullOrWhiteSpace(githubOutputFile))
        {
            using var textWriter = new StreamWriter(githubOutputFile!, true, Encoding.UTF8);
            textWriter.WriteLine($"{propertyName}<<EOF");
            textWriter.WriteLine(Summary);
            textWriter.WriteLine("EOF");
        }
        else
        {
            Console.WriteLine($"::set-output name={propertyName}::{Summary}");
        }
    }
}
