#if NETFRAMEWORK
using Newtonsoft.Json.Linq;
#else
using System.Text.Json;
#endif
using System.Linq;

namespace ChmlFrp.SDK.API;

public abstract class Tunnel
{
    public static async Task<List<string>> GetTunnelNames()
    {
        var result = new List<string>();
        if (!Sign.IsSignin) return result;

        var parameters = new Dictionary<string, string> { { "token", User.Usertoken } };
        var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel", parameters);
        if (jObject == null || (string)jObject["state"] != "success") return [];

#if NETFRAMEWORK
        if (jObject["data"] is not JArray data) return [];
        result.AddRange(data.Select(variable => variable["name"]?.ToString()));
#else
        var data = jObject["data"]?.AsArray();
        if (data == null) return [];
        result.AddRange(data.Select(variable => variable?["name"]?.ToString()));
#endif

        return result;
    }

    public static async Task<TunnelInfo> GetTunnelData(string name)
    {
        if (!Sign.IsSignin) return null;

        var parameters = new Dictionary<string, string> { { "token", User.Usertoken } };
        var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel", parameters);
        if (jObject == null) return null;
        if ((string)jObject["state"] != "success") return null;

#if NETFRAMEWORK
        var data = jObject["data"] as JArray;
        var tunnel = data?.FirstOrDefault(t => t["name"]?.ToString() == name);
        return tunnel?.ToObject<TunnelInfo>();
#else
        var data = jObject["data"]?.AsArray();
        var tunnel = data?.FirstOrDefault(t => t?["name"]?.ToString() == name);
        return tunnel != null
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

        var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel_config", parameters);
        if ((string)jObject["state"] != "success") return null;

        return (string)jObject["data"]!;
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