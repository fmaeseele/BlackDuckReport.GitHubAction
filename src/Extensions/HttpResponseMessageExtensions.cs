using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;

namespace BlackDuckReport.GitHubAction.Extensions;

public static class HttpResponseMessageExtensions
{
    public static bool IsReadableAsStringContent(this HttpResponseMessage httpResponseMessage)
    {
        return httpResponseMessage.IsHtmlContent() || httpResponseMessage.IsTextContent() || httpResponseMessage.IsJsonContent() || httpResponseMessage.IsXmlContent();
    }

    public static bool IsTextContent(this HttpResponseMessage httpResponseMessage)
    {
        return httpResponseMessage.IsMediaTypeContent(MediaTypeNames.Text.Plain);
    }

    public static bool IsXmlContent(this HttpResponseMessage httpResponseMessage)
    {
        return httpResponseMessage.IsMediaTypeContent(MediaTypeNames.Application.Xml);
    }

    public static bool IsJsonContent(this HttpResponseMessage httpResponseMessage)
    {
        return httpResponseMessage.IsMediaTypeContent(MediaTypeNames.Application.Json);
    }

    public static bool IsHtmlContent(this HttpResponseMessage httpResponseMessage)
    {
        return httpResponseMessage.IsMediaTypeContent(MediaTypeNames.Text.Html);
    }

    public static bool IsMediaTypeContent(this HttpResponseMessage httpResponseMessage, string mediaType)
    {
        return httpResponseMessage.Content?.Headers.ContentType?.MediaType == mediaType;
    }

    public static Cookie? GetCookie(this HttpResponseMessage httpResponseMessage, string cookieName)
    {
        if (httpResponseMessage.Headers.TryGetValues("Set-Cookie", out var setCookie))
        {
            return setCookie
                .SelectMany(c => c.Split(';'))
                .Select(token => token.Split('='))
                .Where(keyValueToken => keyValueToken[0] == cookieName)
                .Select(keyValueToken => new Cookie(keyValueToken[0], keyValueToken.Length == 2 ? keyValueToken[1] : null))
                .FirstOrDefault();
        }
        return null;
    }
}
