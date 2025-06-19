using System.IO;
using System.Net.Http;
using System.Text;

namespace BlackDuckReport.GitHubAction.Extensions;

public static class FormUrlEncodedContentExtensions
{
    public static string ReadAsString(this FormUrlEncodedContent form)
    {
        using var reader = new StreamReader(form.ReadAsStream(), Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
