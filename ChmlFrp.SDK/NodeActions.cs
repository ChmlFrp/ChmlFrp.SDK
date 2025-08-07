namespace ChmlFrp.SDK;

public abstract class NodeActions
{
    public static async Task<List<NodeDataClass>> GetNodesDataListAsync()
    {
        var jObject = await GetJsonAsync("https://cf-v2.uapis.cn/node");

        if (jObject == null || (string)jObject["state"] != "success") return [];
        return (from variable in jObject["data"]?.AsArray()!
            select JsonSerializer.Deserialize<NodeDataClass>(variable!.ToJsonString())!).ToList();
    }

    public static async Task<NodeInfoClass> GetNodeInfoAsync
    (
        string nodeName
    )
    {
        // 这个方法获取的节点信息相比上一个更详细（API就是这样的）
        // 具体请看 NodeInfoClass 的定义
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