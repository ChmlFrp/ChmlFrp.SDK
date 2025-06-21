#if NETFRAMEWORK
using Newtonsoft.Json.Linq;
#else
using System.Text.Json;
using System.Text.Json.Nodes;
#endif
using System;
using System.Linq;

namespace ChmlFrp.SDK.API;

public abstract class Tunnel
{
    public static async Task<List<string>> GetTunnelNames()
    {
        if (!Sign.IsSignin) return [];
        var result = new List<string>();

        var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel", new Dictionary<string, string>
        {
            { "token", User.Usertoken }
        });

        if (jObject == null || (string)jObject["state"] != "success") return [];

#if NETFRAMEWORK
        if (jObject["data"] is not JArray data) return [];
        result.AddRange(data.Select(variable => variable["name"]?.ToString()));
#else
        if (jObject["data"] is not JsonArray data) return [];
        result.AddRange(data.Select(variable => variable?["name"]?.GetValue<string>()));
#endif

        return result;
    }

    public static async Task<List<TunnelInfo>> GetTunnelsData()
    {
        if (!Sign.IsSignin) return [];

        var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel", new Dictionary<string, string>
        {
            { "token", User.Usertoken }
        });

        if (jObject == null || (string)jObject["state"] != "success") return [];

#if NETFRAMEWORK
        if (jObject["data"] is not JArray data) return [];
        return data.Select(t => t.ToObject<TunnelInfo>()).Where(t => t != null).ToList();
#else
    if (jObject["data"] is not JsonArray data) return [];
    return data
        .Select(t => t == null ? null : JsonSerializer.Deserialize<TunnelInfo>(t.ToJsonString()))
        .Where(t => t != null)
        .ToList();
#endif
    }

    public static async Task<TunnelInfo> GetTunnelData(string name)
    {
        if (!Sign.IsSignin) return null;

        var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel", new Dictionary<string, string>
        {
            { "token", User.Usertoken }
        });

        if (jObject == null && (string)jObject["state"] != "success") return null;

#if NETFRAMEWORK
        return jObject["data"]?.FirstOrDefault(t => t["name"]?.ToString() == name)?.ToObject<TunnelInfo>();
#else
        if (jObject["data"] is not JsonArray data) return null;
        return JsonSerializer.Deserialize<TunnelInfo>(data.FirstOrDefault(t => t?["name"]?.ToString() == name)!
            .ToJsonString());
#endif
    }

    public static async Task<string> GetTunnelIniData(string name)
    {
        if (!Sign.IsSignin) return null;

        var tunnelData = await GetTunnelData(name);
        if (tunnelData == null) return null;

        var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel_config", new Dictionary<string, string>
        {
            { "token", $"{User.Usertoken}" },
            { "node", $"{tunnelData.node}" },
            { "tunnel_names", $"{name}" }
        });

        if ((string)jObject["state"] != "success") return null;

        return (string)jObject["data"]!;
    }

    public class TunnelInfo
    {
        public int id { get; set; } // 隧道id
        public string ip { get; set; } // 隧道ip
        public string dorp { get; set; } // 外网端口/连接域名
        public string name { get; set; } // 隧道名
        public string node { get; set; } // 所属节点
        public string state { get; set; } // 隧道状态
        public string nodestate { get; set; } // 隧道状态
        public string type { get; set; } // 隧道类型
        public string localip { get; set; } // 内网ip
        public int nport { get; set; } // 内网端口
        public int today_traffic_in { get; set; } // 今日该隧道上传流量
        public int today_traffic_out { get; set; } // 今日该隧道下载流量
        public int cur_conns { get; set; } // 当前隧道连接数
    }

    public static async void DeleteTunnel(string tunnelName)
    {
        if (!Sign.IsSignin) return;
        var tunnelData = await GetTunnelData(tunnelName);
        if (tunnelData == null) return;
        await GetApi("https://cf-v1.uapis.cn/api/deletetl.php", new Dictionary<string, string>
        {
            { "token", User.Usertoken },
            { "userid", User.Userid },
            { "nodeid", tunnelData.id.ToString() }
        });
    }
    
    public static async Task<string> CreateTunnel(string tunnelName, string nodeName, string type,string localport,string remoteport)
    {
        if (!Sign.IsSignin) return null;
        var jObject = await GetApi("https://cf-v1.uapis.cn/api/tunnel.php", new Dictionary<string, string>
        {
            { "token", User.Usertoken },
            { "userid", User.Userid},
            { "name", tunnelName },
            { "node", nodeName },
            { "type", type },
            { "localip","127.0.0.1" },
            { "nport", localport },
            { "dorp", remoteport },
            { "encryption", "false" },
            { "compression", "false" },
            { "ap" , "cat2" }
        });
        return jObject["error"]?.ToString();
    }
}