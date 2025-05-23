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
    public static async Task<object> GetApi(string url, Dictionary<string, string> parameters = null)
    {
        var content = await GetApiConTent(url, parameters);
        if (content == null) return null;

#if NETFRAMEWORK
        return JObject.Parse(content);
#else
        return JsonNode.Parse(content);
#endif
    }

    public static async Task<string> GetApiConTent(string url, Dictionary<string, string> parameters = null)
    {
        if (parameters != null)
            url = $"{url}?{string.Join("&", parameters.Select(pair => $"{pair.Key}={pair.Value}"))}";

        try
        {
            HttpClient client = new();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            client.Dispose();
            return await response.Content.ReadAsStringAsync();
        }
        catch
        {
            return null;
        }
    }
}