using System.Net.Http;

namespace ChmlFrp.SDK;

public abstract class HttpActions
{
    private static readonly HttpClient Client = new()
    {
        Timeout = TimeSpan.FromSeconds(3)
    };

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
            var queryString = await new FormUrlEncodedContent(parameters).ReadAsStringAsync();
            using var response = await Client.GetAsync($"{url}?{queryString}");
            response.EnsureSuccessStatusCode();
            return JsonNode.Parse(await response.Content.ReadAsStringAsync());
        }
        catch
        {
            return null;
        }
    }
}