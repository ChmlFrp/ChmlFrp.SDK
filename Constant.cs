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
            var fileData = await Client.GetByteArrayAsync(url);
            File.WriteAllBytes(path, fileData);
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            return false;
        }

        return true;
    }
}