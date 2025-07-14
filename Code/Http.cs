using System.Net.Http;
using System.Text;

namespace CSDK;

public abstract class Http
{
    private static readonly HttpClient Client = new();

    public static async Task<JsonNode> GetJsonAsync
    (
        string url
    )
    {
        try
        {
            using var response = await Client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            return null;
        }
    }

    public static async Task<JsonNode> GetJsonAsync
    (
        string url,
        Dictionary<string, string> parameters
    )
    {
        try
        {
            using var response =
                await Client.GetAsync(
                    $"{url}?{string.Join("&", parameters.Select(pair => $"{pair.Key}={pair.Value}"))}");
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            return null;
        }
    }

    public static async Task<JsonNode> PostJsonAsync
    (
        string url,
        string jsonContent
    )
    {
        try
        {
            using var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            using var response = await Client.PostAsync(url, content);
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            return null;
        }
    }

    public static async Task<bool> GetFileAsync
    (
        string url,
        string path
    )
    {
        try
        {
#if NET
            await File.WriteAllBytesAsync(path, await Client.GetByteArrayAsync(url));
#else
            File.WriteAllBytes(path, await Client.GetByteArrayAsync(url));
#endif
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
        }

        return File.Exists(path) && new FileInfo(path).Length != 0;
    }
}