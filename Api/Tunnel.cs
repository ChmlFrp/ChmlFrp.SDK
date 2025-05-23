using System.Linq;
#if NETFRAMEWORK
using Newtonsoft.Json.Linq;

#else
using System.Text.Json;
using System.Text.Json.Nodes;
#endif

namespace ChmlFrp.SDK.API;

public abstract class Tunnel
{
    public static async Task<List<string>> GetTunnelNames()
    {
        var result = new List<string>();
        if (!Sign.IsSignin) return result;

        var parameters = new Dictionary<string, string> { { "token", User.Usertoken } };
        var jObject = await Constant.GetApi("https://cf-v2.uapis.cn/tunnel", parameters);
        if (jObject == null) return result;

#if NETFRAMEWORK
        var msg = ((JObject)jObject)["msg"]?.ToString();
        if (msg != "获取隧道数据成功") return result;
        var data = ((JObject)jObject)["data"] as JArray;
        if (data == null) return result;
        foreach (var item in data)
        {
            var name = item["name"]?.ToString();
            if (!string.IsNullOrEmpty(name))
                result.Add(name);
        }
#else
    var msg = ((JsonNode)jObject)?["msg"]?.ToString();
    if (msg != "获取隧道数据成功") return result;
    var data = ((JsonNode)jObject)?["data"]?.AsArray();
    if (data == null) return result;
    foreach (var item in data)
    {
        var name = item?["name"]?.ToString();
        if (!string.IsNullOrEmpty(name))
            result.Add(name);
    }
#endif
        return result;
    }

    public static async Task<TunnelInfo> GetTunnelData(string name)
    {
        if (!Sign.IsSignin) return null;

        var parameters = new Dictionary<string, string> { { "token", User.Usertoken } };
        var jObject = await Constant.GetApi("https://cf-v2.uapis.cn/tunnel", parameters);
        if (jObject == null) return null;

#if NETFRAMEWORK
        var msg = ((JObject)jObject)["msg"]?.ToString();
        if (msg != "获取隧道数据成功") return null;
        var data = ((JObject)jObject)["data"] as JArray;
        var tunnel = data?.FirstOrDefault(t => t["name"]?.ToString() == name);
        return tunnel?.ToObject<TunnelInfo>();
#else
    var msg = ((JsonNode)jObject)?["msg"]?.ToString();
    if (msg != "获取隧道数据成功") return null;
    var data = ((JsonNode)jObject)?["data"]?.AsArray();
var tunnel = data?.FirstOrDefault(t => t?["name"]?.ToString() == name);
return tunnel is not null
    ? JsonSerializer.Deserialize<TunnelInfo>(tunnel.ToJsonString())
    : null;
#endif
    }

    public static async Task<string> GetTunnelIniData(string name)
    {
        if (!Sign.IsSignin) return null;

        var tunnelData = await GetTunnelData(name);
        if (tunnelData == null) return null;

        var parameters = new Dictionary<string, string>
        {
            { "token", $"{User.Usertoken}" },
            { "node", $"{tunnelData.node}" },
            { "tunnel_names", $"{name}" }
        };
        var jObject = await Constant.GetApi("https://cf-v2.uapis.cn/tunnel_config", parameters);

#if NETFRAMEWORK
        var data = ((JObject)jObject)["data"];
        var msg = ((JObject)jObject)["msg"];
#else
        var data = ((JsonNode)jObject)?["data"];
        var msg = ((JsonNode)jObject)?["msg"];
#endif
        if (msg?.ToString() != "配置文件获取成功") return null;
        return (string)data!;
    }

    public class TunnelInfo
    {
        public int id { get; set; } // 隧道id
        public string name { get; set; } // 隧道名
        public string node { get; set; } // 所属节点
        public bool state { get; set; } // 隧道状态
        public string type { get; set; } // 隧道类型
        public string localip { get; set; } // 内网ip
        public int nport { get; set; } // 内网端口
        public string dorp { get; set; } // 外网端口/连接域名
        public int today_traffic_in { get; set; } // 今日该隧道上传流量
        public int today_traffic_out { get; set; } // 今日该隧道下载流量
        public int cur_conns { get; set; } // 当前隧道连接数
    }
}