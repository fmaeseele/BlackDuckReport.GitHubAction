using System;
using System.CommandLine;

using Microsoft.Extensions.Logging;

namespace BlackDuckReport.GitHubAction;

public class ActionInputs
{
    private readonly RootCommand root;
    private static readonly Option<string> projectNameOption;
    private static readonly Option<Version> projectVersionOption;
    private static readonly Option<string> blackDuckApiTokenOption;
    private static readonly Option<Uri> blackDuckUrlOption;

    static ActionInputs()
    {
        projectNameOption = new Option<string>("--project_name", "-p")
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne,
            Required = true,
        };

        projectVersionOption = new Option<Version>("--project_version", "-f")
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne,
            Required = false,
            CustomParser = result =>
            {
                if (result.Tokens.Count != 1)
                {
                    result.AddError("--project_version requires one argument");
                    return null;
                }
                if (!Version.TryParse(result.Tokens[0].Value, out var project_version))
                {
                    result.AddError($"--project_version argument is not a valid version: {result.Tokens[0].Value}");
                    return null;
                }
                return project_version;
            }
        };

        blackDuckApiTokenOption = new Option<string>("--blackduck_token", "-t")
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne,
            Required = true
        };

        blackDuckUrlOption = new Option<Uri>("--blackduck_url", "-u")
        {
            Description = "",
            Arity = ArgumentArity.ExactlyOne,
            Required = true,
            CustomParser = result =>
            {
                if (result.Tokens.Count != 1)
                {
                    result.AddError("--blackduck_url requires one argument");
                    return null;
                }
                if (!Uri.TryCreate(result.Tokens[0].Value, UriKind.Absolute, out var blackduck_url))
                {
                    result.AddError($"--blackduck_url argument is not a valid url: {result.Tokens[0].Value}");
                    return null;
                }
                return blackduck_url;
            }
        };
    }

    public ActionInputs()
    {
        root = [];
        root.Options.Add(projectNameOption);
        root.Options.Add(projectVersionOption);
        root.Options.Add(blackDuckApiTokenOption);
        root.Options.Add(blackDuckUrlOption);
    }

    public bool Parse(string[] args, ILogger logger)
    {
        var result = root.Parse(args);

        if (result.Errors?.Count > 0)
        {
            foreach (var err in result.Errors)
            {
                logger.LogError("{error}", err.Message);
            }
            return false;
        }

        ProjectName = result.GetValue(projectNameOption);
        ProjectVersion = result.GetValue(projectVersionOption);
        BlackDuckToken = result.GetValue(blackDuckApiTokenOption);
        BlackDuckUrl = result.GetValue(blackDuckUrlOption);

        return true;
    }

    public string? ProjectName { get; private set; }

    public Version? ProjectVersion { get; private set; }

    public string? BlackDuckToken { get; private set; }

    public Uri? BlackDuckUrl { get; private set; }
}
