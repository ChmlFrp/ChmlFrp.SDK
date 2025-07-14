namespace CSDK;

public abstract class NodeActions
{
    public static async Task<List<NodeDataClass>> GetNodesData()
    {
        var jObject = await GetJsonAsync("https://cf-v2.uapis.cn/node");

        if (jObject == null || (string)jObject["state"] != "success") return [];
        return (from variable in jObject["data"]?.AsArray()!
            select JsonSerializer.Deserialize<NodeDataClass>(variable!.ToJsonString())!).ToList();
    }

    public static async Task<NodeInfoClass> GetNodeInfo
    (
        string nodeName
    )
    {
        var jObject = await GetJsonAsync("https://cf-v2.uapis.cn/nodeinfo", new Dictionary<string, string>
        {
            {
                "token", UserActions.Usertoken
            },
            {
                "node", nodeName
            }
        });

        if (jObject == null || (string)jObject["state"] != "success") return null;
        return JsonSerializer.Deserialize<NodeInfoClass>(jObject["data"]!.ToJsonString());
    }
}