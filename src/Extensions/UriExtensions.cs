using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web;

namespace BlackDuckReport.GitHubAction.Extensions;

public static class UriExtensions
{
    /// <summary>
    /// Add Query Parameters to existing Uri
    /// </summary>
    /// <param name="uri"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    public static Uri AddQueryParameters(this Uri uri, KeyValueList<string, string>? queryParameters)
    {
        Uri _uri = uri;
        if (queryParameters is not null)
        {
            KeyValueList<string, string> parameters = [];

            // Copy queryParameters
            foreach (KeyValuePair<string, string> kvp in queryParameters)
                parameters.Add(kvp.Key, kvp.Value);

            // Copy existing uri parameters
            var uriParameters = HttpUtility.ParseQueryString(uri.Query);
            foreach (string key in uriParameters.Keys)
                parameters.Add(key, uriParameters[key]!);

            // Build new RequestUri with all parameters
            using var form = new FormUrlEncodedContent(parameters);
            var builder = new UriBuilder()
            {
                // Get Escaped query string
                Query = form.ReadAsString()
            };

            // Update HttpRequest
            _uri = new Uri(uri, builder.Query);
        }
        return _uri;
    }
}
