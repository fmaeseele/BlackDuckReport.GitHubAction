using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

using BlackDuckReport.GitHubAction.Extensions;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace BlackDuckReport.GitHubAction.Http;

public partial class RestClient : IDisposable
{
    private readonly ILogger<RestClient>? _logger;
    private readonly HttpClient _httpClient;
    private bool disposedValue;

    public RestConfiguration Configuration { get; }

    public RestClient(IServiceProvider serviceProvider, [Optional] RestConfiguration? configuration)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);
        _logger = serviceProvider.GetService<ILogger<RestClient>>();

        Configuration = configuration ?? new();

        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All,
            Proxy = Configuration.UseFiddlerProxy ? new WebProxy(IPAddress.Loopback.ToString(), 8888) : null,
            UseProxy = Configuration.UseFiddlerProxy,
        };

        // Add Client Certificates for Authorization
        if (Configuration.Certificates is not null)
            handler.ClientCertificates.AddRange(Configuration.Certificates.ToArray());

        _httpClient = new HttpClient(handler, true)
        {
            BaseAddress = Configuration.BaseAddress
        };

        if (!string.IsNullOrEmpty(Configuration.UserAgent))
        {
            _httpClient.DefaultRequestHeaders.UserAgent.Clear();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(Configuration.UserAgent);
        }
    }

    #region IDisposable

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
            }

            // TODO: free unmanaged resources (unmanaged objects) and override finalizer
            // TODO: set large fields to null

            _logger?.LogDebug("Disposing httpClient...");
            _httpClient.CancelPendingRequests();
            _httpClient.Dispose();

            disposedValue = true;
        }
    }

    // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
    // ~RestClient()
    // {
    //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
    //     Dispose(disposing: false);
    // }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    #endregion

    /// <summary>
    /// Create POST HttpMessage
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="queryUri">relative Uri</param>
    /// <param name="queryData">payload to post</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HttpRequestMessage CreatePostHttpMessage(string queryUri, [Optional] object? queryData)
    {
        if (string.IsNullOrEmpty(queryUri)) throw new ArgumentNullException(nameof(queryUri));

        var uri = new Uri(queryUri, UriKind.RelativeOrAbsolute);

        HttpContent? data;
        if (queryData is FormUrlEncodedContent form)
        {
            data = form;
        }
        else
        {
            data = null;
        }

        return new HttpRequestMessage()
        {
            RequestUri = uri,
            Method = HttpMethod.Post,
            Content = data,
        };
    }

    /// <summary>
    /// Create GET HttpMessage
    /// </summary>
    /// <param name="httpClient"></param>
    /// <param name="queryUri">relative Uri</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static HttpRequestMessage CreateGetHttpMessage(string queryUri)
    {
        ArgumentNullException.ThrowIfNull(queryUri);

        var uri = new Uri(queryUri, UriKind.RelativeOrAbsolute);

        return new HttpRequestMessage()
        {
            RequestUri = uri,
            Method = HttpMethod.Get
        };
    }


    public async Task<TValue> SendJsonMessageAsync<TValue, TError>(HttpRequestMessage httpRequestMessage, JsonTypeInfo<TValue> jsonTypeInfoValue, JsonTypeInfo<TError> jsonTypeInfoError, bool checkIfJsonContent = true, bool throwJsonContentOnError = true, CancellationToken cancellationToken = default)
    where TValue : class
    where TError : class
    {
        var response = await SendHttpMessageAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);

        // Check ResponseCode
        if (!throwJsonContentOnError)
        {
            _ = response.EnsureSuccessStatusCode();
        }
        else if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            _logger?.LogError("Error={response.StatusCode} Message={response.ReasonPhrase}", response.StatusCode, response.ReasonPhrase);
            throw new UnauthorizedAccessException(response.ReasonPhrase);
        }

        // Check if Json content
        // Note: Sometimes, api are returning that content-type is something else than json,
        //       Hence, the checkIfJsonContent parameter to avoid exception to be raised.
        if (checkIfJsonContent && !response.IsJsonContent())
            throw new InvalidOperationException("Not Json Content");

        if (response.IsSuccessStatusCode)
        {
            // Deserialize Json Content
            var result = await response.Content.ReadFromJsonAsync(jsonTypeInfoValue, cancellationToken).ConfigureAwait(false);
            return result is null ? throw new InvalidOperationException("ReadFromJsonAsync returned null") : result;
        }
        else
        {
            var result = await response.Content.ReadFromJsonAsync(jsonTypeInfoError, cancellationToken).ConfigureAwait(false) ?? throw new InvalidOperationException("ReadFromJsonAsync returned null");
            throw new RestException<TError>(result);
        }
    }

    public async Task<HttpResponseMessage> SendHttpMessageAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(httpRequestMessage);

        if (httpRequestMessage.RequestUri!.IsAbsoluteUri)
            _logger?.LogTrace("Contacting endpoint {httpRequestMessage.RequestUri}", httpRequestMessage.RequestUri);
        else
            _logger?.LogTrace("Contacting endpoint {_httpClient.BaseAddress}{httpRequestMessage.RequestUri}", _httpClient.BaseAddress, httpRequestMessage.RequestUri);

        // Dump Request Headers
        {
            using var _loggerScope = _logger?.BeginScope("Dump HttpRequest");

            foreach (var header in httpRequestMessage.Headers)
            {
                _logger?.LogTrace("HttpRequest {Key}={value}", header.Key, string.Join(',', header.Value));
            }

            // Dump Payload
            if (httpRequestMessage.Content is not null)
            {
                var requestContent = await httpRequestMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger?.LogTrace("HttpRequest Content-Type={httpRequestMessage.Content.Headers.ContentType} Content={requestContent}", httpRequestMessage.Content.Headers.ContentType, requestContent);
            }
        }

        // Send HttpRequest
        var _loggerScopeSend = _logger?.BeginScope("Send HttpRequest");
        var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        _loggerScopeSend?.Dispose();

        // Dump Response Headers
        {
            using var _loggerScope = _logger?.BeginScope("Dump HttpResponse");
            _logger?.LogTrace("Endpoint {RequestUri} Response Code={StatusCode} Response ContentType={ContentType}", response.RequestMessage!.RequestUri, response.StatusCode, response.Content.Headers.ContentType);

            foreach (var header in response.Headers)
            {
                _logger?.LogTrace("HttpResponse {Key}={value}", header.Key, string.Join(',', header.Value));
            }

            // Dump Body
            if (response.IsReadableAsStringContent())
            {
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                _logger?.LogTrace("HttpResponse Content={responseContent}", responseContent);
            }
        }

        return response;
    }
}
