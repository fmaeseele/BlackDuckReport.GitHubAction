namespace BlackDuckReport.GitHubAction.Api;

public partial class BlackDuckApi
{
    public sealed class Constants
    {
        public const string HTTP_ACCEPT_JSON = "application/vnd.blackducksoftware.user-4+json";
        public const string HTTP_AUTHORIZATION_TOKEN = "token ";
        public const string HTTP_LOGIN_WITH_TOKEN_URL = "/api/tokens/authenticate";
        public const string HTTP_PROJECT_VERSIONS_URL = "/api/search/project-versions";
    }
}
