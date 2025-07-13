using System.Linq;
using System.Text.Json;

namespace ChmlFrp.SDK.API;

public abstract class Node
{
    public static async Task<List<NodeData>> GetNodesData()
    {
        var jObject = await GetApi("https://cf-v2.uapis.cn/node");
        if (jObject == null || (string)jObject["state"] != "success") return [];
        return (from variable in jObject["data"]?.AsArray()!
            select JsonSerializer.Deserialize<NodeData>(variable!.ToJsonString())!).ToList();
    }

    public static async Task<NodeInfo> GetNodeInfo(string name)
    {
        var jObject = await GetApi("https://cf-v2.uapis.cn/nodeinfo", new Dictionary<string, string>
        {
            { "token", User.Usertoken }, { "node", name }
        });
        if (jObject == null || (string)jObject["state"] != "success") return null;
        return JsonSerializer.Deserialize<NodeInfo>(jObject["data"]!.ToJsonString());
    }

    public class NodeData
    {
        public long id { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public string nodegroup { get; set; }
        public string china { get; set; }
        public string web { get; set; }
        public string udp { get; set; }
    }

    public class NodeInfo : NodeData
    {
        public string ip { get; set; }
        public string location { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string version { get; set; }
        public string uptime { get; set; }
        public int load { get; set; }
        public int users { get; set; }
        public int bandwidth { get; set; }
        public int traffic { get; set; }
        public int port { get; set; }
        public int adminPort { get; set; }
        public int memory_total { get; set; }
        public int storage_total { get; set; }
        public int storage_used { get; set; }
        public int total_traffic_in { get; set; }
        public int total_traffic_out { get; set; }
        public string cpu_info { get; set; }
        public string nodetoken { get; set; }
        public string realIp { get; set; }
        public string rport { get; set; }
        public bool toowhite { get; set; }
    }
}