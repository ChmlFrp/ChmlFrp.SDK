using System.IO;
using System.Linq;
using System.Net.Http;
#if NETFRAMEWORK
using Newtonsoft.Json.Linq;
#else
using System.Text.Json.Nodes;
#endif

namespace ChmlFrp.SDK.API;

public abstract class Http
{
    private static readonly HttpClient Client = new();

#if NETFRAMEWORK
    public static async Task<JObject> GetApi(string url, Dictionary<string, string> parameters = null)
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

        return JObject.Parse(content);
    }
#else
    public static async Task<JsonNode> GetApi(string url, Dictionary<string, string> parameters = null)
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

        return JsonNode.Parse(content);
    }
#endif


    public static async Task<bool> GetFile(string url, string path)
    {
        try
        {
#if NETFRAMEWORK
            File.WriteAllBytes(path, await Client.GetByteArrayAsync(url));
#elif NET
            await File.WriteAllBytesAsync(path, await Client.GetByteArrayAsync(url));
#endif
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
        }

        return File.Exists(path) && new FileInfo(path).Length != 0;
    }
}