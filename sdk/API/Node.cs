#if NETFRAMEWORK
using Newtonsoft.Json.Linq;

#else
using System.Text.Json;
#endif

namespace ChmlFrp.SDK.API;

public abstract class Node
{
    public static async Task<List<Person>> GetNodeData()
    {
        var jObject = await GetApi("https://cf-v2.uapis.cn/node");
        if (jObject == null || (string)jObject["state"] != "success") return [];
        var list = new List<Person>();

#if NETFRAMEWORK
        foreach (var variable in jObject["data"])
            list.Add(variable.ToObject<Person>());
#elif NET
        foreach (var variable in jObject["data"]?.AsArray()!)
            list.Add(JsonSerializer.Deserialize<Person>(variable.ToJsonString())!);
#endif

        return list;
    }

    public class Person
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