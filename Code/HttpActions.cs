using System.Net.Http;

namespace CSDK;

public abstract class HttpActions
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
            // 比较原始的Url处理方式
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