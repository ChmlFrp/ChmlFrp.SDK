using System.IO;
using System.Linq;
using System.Net.Http;
#if NETFRAMEWORK
using Newtonsoft.Json.Linq;

#else
using System.Text.Json.Nodes;
#endif

namespace ChmlFrp.SDK;

public abstract class Constant
{
    private static readonly HttpClient Client = new();

    public static async Task<object> GetApi(string url, Dictionary<string, string> parameters = null)
    {
        if (parameters != null)
            url = $"{url}?{string.Join("&", parameters.Select(pair => $"{pair.Key}={pair.Value}"))}";

        string content;
        try
        {
            var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            content = await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return null;
        }

#if NETFRAMEWORK
        return JObject.Parse(content);
#else
        return JsonNode.Parse(content);
#endif
    }

    public static async Task<bool> GetFile(string url, string path)
    {
        try
        {
            var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var stream = await response.Content.ReadAsStreamAsync();
            var fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.CopyToAsync(fileStream);
        }
        catch
        {
            return false;
        }

        return true;
    }
}