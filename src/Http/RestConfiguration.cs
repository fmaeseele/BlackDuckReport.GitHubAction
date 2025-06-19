using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace BlackDuckReport.GitHubAction.Http;

public sealed class RestConfiguration
{
    public Uri? BaseAddress { get; set; }
    public bool UseFiddlerProxy { get; set; }
    public string? UserAgent { get; set; }
    public List<X509Certificate2>? Certificates { get; set; }
}