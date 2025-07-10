using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;

namespace ChmlFrp.SDK.API;

public abstract class Http
{
    private static readonly HttpClient Client = new();

    public static async Task<JsonNode> GetApi(string url, Dictionary<string, string> parameters = null)
    {
        if (parameters != null)
            url = $"{url}?{string.Join("&", parameters.Select(pair => $"{pair.Key}={pair.Value}"))}";

        string content;
        try
        {
            using var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            content = await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return null;
        }

        return JsonNode.Parse(content);
    }


    public static Task<bool> GetFile(string url, string path)
    {
        try
        {
#if NET
            await File.WriteAllBytesAsync(path, await Client.GetByteArrayAsync(url));
#else
            File.WriteAllBytes(path, Client.GetByteArrayAsync(url).Result);
#endif
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
        }

        return Task.FromResult(File.Exists(path) && new FileInfo(path).Length != 0);
    }
}