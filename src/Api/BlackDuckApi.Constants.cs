namespace BlackDuckReport.GitHubAction.Api;

public partial class BlackDuckApi
{
    public sealed class Constants
    {
        public const string HTTP_ACCEPT_JSON = "application/vnd.blackducksoftware.internal-1+json, application/json, */*;q=0.8";
        public const string HTTP_AUTHORIZATION_TOKEN = "token ";
        public const string HTTP_LOGIN_WITH_TOKEN_URL = "/api/tokens/authenticate";
        public const string HTTP_SEARCH_PROJECT_VERSIONS_URL = "/api/search/project-versions";
    }
}
