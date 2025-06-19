using System;

namespace BlackDuckReport.GitHubAction.Http;

public sealed class RestException<T> : Exception where T : class
{
    public T Error { get; }

    public RestException(T error)
    {
        Error = error;
    }
}
