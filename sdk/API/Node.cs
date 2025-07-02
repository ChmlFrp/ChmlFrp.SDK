using System.Linq;
using System.Text.Json;

namespace ChmlFrp.SDK.API;

public abstract class Node
{
    public static async Task<List<NodeData>> GetNodeData()
    {
        var jObject = await GetApi("https://cf-v2.uapis.cn/node");
        if (jObject == null || (string)jObject["state"] != "success") return [];
        return (from variable in jObject["data"]?.AsArray()! select JsonSerializer.Deserialize<NodeData>(variable.ToJsonString())!).ToList();
    }

    public class NodeData
    {
        public int id { get; set; }
        public string name { get; set; }
        public string notes { get; set; }
        public string nodegroup { get; set; }
        public string china { get; set; }
        public string web { get; set; }
        public string udp { get; set; }
    }
}