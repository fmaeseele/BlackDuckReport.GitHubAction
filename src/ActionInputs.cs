using System;

using CommandLine;

namespace BlackDuckReport.GitHubAction;

public class ActionInputs
{
    public ActionInputs()
    {
    }

    [Option('p', "project_name",
        Required = true,
        HelpText = "The BlackDuck project name.")]
    public string? ProjectName { get; set; }

    [Option('f', "project_version",
        Required = true,
        HelpText = "The BlackDuck project version.")]
    public Version? ProjectVersion { get; set; }

    [Option('t', "blackduck_token",
        Required = true,
        HelpText = "The BlackDuck api token.")]
    public string? BlackDuckToken { get; set; }

    [Option('u', "blackduck_url",
        Required = true,
        HelpText = "The BlackDuck server url.")]
    public Uri? BlackDuckUrl { get; set; }
}
