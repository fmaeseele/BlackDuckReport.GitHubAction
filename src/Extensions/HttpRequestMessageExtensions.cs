using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;

using BlackDuckReport.GitHubAction.Http;

namespace BlackDuckReport.GitHubAction.Extensions;

public class KeyValueList<TKey, TValue> : List<KeyValuePair<TKey, TValue>>
{
    public void Add(TKey key, TValue value)
    {
        Add(new KeyValuePair<TKey, TValue>(key, value));
    }
}

public static class HttpRequestMessageExtensions
{
    /// <summary>
    /// Ask the HttpRequestMessage to accept specific content
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static HttpRequestMessage AcceptContent(this HttpRequestMessage request, string contentType)
    {
        if (string.IsNullOrEmpty(contentType)) throw new ArgumentNullException(nameof(contentType));
        request.Headers.Accept.ParseAdd(contentType);
        return request;
    }

    /// <summary>
    /// Ask the HttpRequestMessage to accept Json content
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public static HttpRequestMessage AcceptJsonContent(this HttpRequestMessage request)
    {
        request.Headers.Accept.ParseAdd(MediaTypeNames.Application.Json);
        return request;
    }

    /// <summary>
    /// Modify request uri with queryParameters
    /// 
    /// From KeyValuePair<string,string> list, generate Query ui part: ?key1=value1&key2=value2&key3=value3
    /// Using KeyValuePair instead of Dictionary to allow same key to be used more than once.
    /// </summary>
    /// <param name="request"></param>
    /// <param name="queryParameters"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public static HttpRequestMessage WithQueryParameters(this HttpRequestMessage request, KeyValueList<string, string>? queryParameters)
    {
        ArgumentNullException.ThrowIfNull(request.RequestUri);

        if (queryParameters is not null)
        {
            using var form = new FormUrlEncodedContent(queryParameters);

            // Build new RequestUri with Query parameters
            var builder = new UriBuilder()
            {
                // Get Escaped query string
                Query = form.ReadAsString()
            };

            // Update HttpRequest
            request.RequestUri = new Uri(request.RequestUri.ToString() + builder.Query, UriKind.RelativeOrAbsolute);
        }
        return request;
    }

    /// <summary>
    /// Add Bearer authorization field to HttpRequestMessage
    /// </summary>
    /// <param name="request"></param>
    /// <param name="bearerToken"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HttpRequestMessage WithBearerToken(this HttpRequestMessage request, string? bearerToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(bearerToken);

        request.Headers.Authorization = new BearerAuthenticationHeaderValue(bearerToken);
        return request;
    }

    /// <summary>
    /// Add Basic authorization field to HttpRequestMessage
    /// </summary>
    /// <param name="request"></param>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HttpRequestMessage WithBasicCredentials(this HttpRequestMessage request, string? username, string? password)
    {
        ArgumentException.ThrowIfNullOrEmpty(username);
        ArgumentException.ThrowIfNullOrEmpty(password);

        request.Headers.Authorization = new BasicAuthenticationHeaderValue(username, password);
        return request;
    }

    /// <summary>
    /// Add Authorization field to HttpRequestMessage
    /// </summary>
    /// <param name="request"></param>
    /// <param name="authorization"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HttpRequestMessage WithAuthorization(this HttpRequestMessage request, string? authorization)
    {
        ArgumentException.ThrowIfNullOrEmpty(authorization);

        if (!request.Headers.TryAddWithoutValidation("Authorization", authorization))
            throw new InvalidOperationException();
        return request;
    }

    /// <summary>
    /// Add UserAgent field to HttpRequestMessage
    /// </summary>
    /// <param name="request"></param>
    /// <param name="userAgent"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static HttpRequestMessage WithUserAgent(this HttpRequestMessage request, string? userAgent)
    {
        ArgumentException.ThrowIfNullOrEmpty(userAgent);

        request.Headers.UserAgent.Clear();
        if (!request.Headers.UserAgent.TryParseAdd(userAgent))
            throw new InvalidOperationException();
        return request;
    }

    /// <summary>
    /// Add Referer field to HttpRequestMessage
    /// </summary>
    /// <param name="request"></param>
    /// <param name="referer"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public static HttpRequestMessage WithReferer(this HttpRequestMessage request, Uri referer)
    {
        ArgumentNullException.ThrowIfNull(referer);

        request.Headers.Referrer = referer;
        return request;
    }
}
