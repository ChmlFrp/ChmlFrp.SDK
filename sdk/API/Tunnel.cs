using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ChmlFrp.SDK.API;

public abstract class Tunnel
{
    public static async Task<List<TunnelInfo>> GetTunnelsData()
    {
        if (!Sign.IsSignin) return [];

        var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel", new Dictionary<string, string>
        {
            { "token", User.Usertoken }
        }).ConfigureAwait(false);

        if (jObject?["state"]?.ToString() != "success" || jObject["data"] is not JsonArray data) return [];

        var result = new List<TunnelInfo>(data.Count);
        result.AddRange(from t in data where t != null select JsonSerializer.Deserialize<TunnelInfo>(t.ToJsonString()) into info where info != null select info);
        return result;
    }

    public static async Task<TunnelInfo> GetTunnelData(string name)
    {
        if (!Sign.IsSignin) return null;

        var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel", new Dictionary<string, string>
        {
            { "token", User.Usertoken }
        });

        if (jObject == null || (string)jObject["state"] != "success") return null;

        if (jObject["data"] is not JsonArray data) return null;
        return JsonSerializer.Deserialize<TunnelInfo>(data.FirstOrDefault(t => t?["name"]?.ToString() == name)!
            .ToJsonString());
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

    public static async Task<string> CreateTunnel(string nodeName, string type, string localport, string remoteport)
    {
        if (!Sign.IsSignin) return null;

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new char[8];
        for (var i = 0; i < 8; i++)
            result[i] = chars[random.Next(chars.Length)];
        var tunnelName = new string(result);
        
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
            { "ap" , "" }
        });
        return jObject["error"]?.ToString();
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
}