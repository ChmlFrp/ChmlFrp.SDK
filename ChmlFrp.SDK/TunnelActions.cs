using System.Diagnostics;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using static ChmlFrp.SDK.SetPath;
using static ChmlFrp.SDK.UserActions;

namespace ChmlFrp.SDK;

public abstract class TunnelActions
{
    # region API Methods

    public static async Task<List<TunnelInfoClass>> GetTunnelListAsync()
    {
        if (!IsLoggedIn)
            return [];

        var jsonNode = await GetJsonAsync("https://cf-v2.uapis.cn/tunnel", new Dictionary<string, string>
        {
            {
                "token", Usertoken
            }
        });

        if (jsonNode == null || (string)jsonNode["state"] != "success") return [];

        var data = (JsonArray)jsonNode["data"];
        var result = new List<TunnelInfoClass>(data!.Count);
        result.AddRange(data.Where(t => t != null)
            .Select(t => JsonSerializer.Deserialize<TunnelInfoClass>(t.ToJsonString()))
            .Where(info => info != null));
        return result;
    }

    public static async Task<bool> DeleteTunnelAsync
    (
        int tunnelId
    )
    {
        StopTunnel(tunnelId);
        var jsonNode = await GetJsonAsync("https://cf-v1.uapis.cn/api/deletetl.php", new Dictionary<string, string>
        {
            {
                "token", Usertoken
            },
            {
                "userid", Userid
            },
            {
                "nodeid", tunnelId.ToString()
            }
        });

        return jsonNode != null && (int)jsonNode["code"] == 200;
    }

    public static async Task<string> CreateTunnelAsync
    (
        string nodeName,
        string type,
        string localIp,
        string localPort,
        string remotePort
    )
    {
        // ReSharper disable once StringLiteralTypo
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var random = new Random();
        var result = new char[8];
        for (var i = 0; i < 8; i++)
            result[i] = chars[random.Next(chars.Length)];
        var tunnelName = new string(result);

        var jsonNode = await GetJsonAsync("https://cf-v1.uapis.cn/api/tunnel.php", new Dictionary<string, string>
        {
            { "token", Usertoken },
            { "userid", Userid },
            { "name", tunnelName },
            { "node", nodeName },
            { "type", type },
            { "localip", localIp },
            { "nport", localPort },
            { "dorp", remotePort },
            { "encryption", "false" },
            { "compression", "false" },
            { "ap", "" }
        });
        return jsonNode["error"]?.ToString();
    }

    public static async Task<string> UpdateTunnelAsync
    (
        TunnelInfoClass tunnelInfo,
        string nodeName,
        string type,
        string localIp,
        string localPort,
        string remotePort
    )
    {
#if WINDOWS
        StopTunnel(tunnelInfo.id);
#endif
        var jsonNode = await GetJsonAsync("https://cf-v1.uapis.cn/api/cztunnel.php", new Dictionary<string, string>
        {
            { "usertoken", Usertoken },
            { "userid", Userid },
            { "tunnelid", tunnelInfo.id.ToString() },
            { "name", tunnelInfo.name },
            { "node", nodeName },
            { "type", type },
            { "localip", localIp },
            { "nport", localPort },
            { "dorp", remotePort },
            { "encryption", "false" },
            { "compression", "false" },
            { "ap", "" }
        });
        return jsonNode["error"]?.ToString();
    }

    [Obsolete("该方法已被弃用，StartTunnel已不在使用。")]
    public static async Task<string> GetIniStringAsync
    (
        TunnelInfoClass tunnelInfo
    )
    {
        var jsonNode = await GetJsonAsync("https://cf-v2.uapis.cn/tunnel_config", new Dictionary<string, string>
        {
            {
                "token", $"{Usertoken}"
            },
            {
                "node", $"{tunnelInfo.node}"
            },
            {
                "tunnel_names", tunnelInfo.name
            }
        });

        if ((string)jsonNode["state"] != "success") return null;
        return (string)jsonNode["data"]!;
    }

    #endregion

    #region Windows Service Methods

    public static void StartTunnel
    (
        TunnelInfoClass tunnelInfo,
        Action onStartTrue = null,
        Action onStartFalse = null,
        Action onTunnelRunning = null
    )
    {
        if (IsTunnelRunning(tunnelInfo.id))
        {
            onTunnelRunning?.Invoke();
            return;
        }

        var frpProcess = new Process
        {
            StartInfo =
            {
                FileName = "cmd.exe",
                Arguments = $"/c cd {AppDomain.CurrentDomain.BaseDirectory} & frpc -u {Usertoken} -p {tunnelInfo.id}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            }
        };

        var logOpened = false;
        var logfilePath = Path.Combine(DataPath, $"{tunnelInfo.name}({tunnelInfo.type.ToUpper()}).log");
        File.WriteAllText(logfilePath, string.Empty);
        frpProcess.OutputDataReceived += async (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(args.Data)) return;
            File.AppendAllText(logfilePath, args.Data.Replace(Usertoken, "{Usertoken}") + Environment.NewLine);

            if (args.Data.Contains("启动配置文件"))
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "frpc.ini"));
                return;
            }

            if (args.Data.Contains("启动成功"))
            {
                onStartTrue?.Invoke();
                return;
            }

            if (logOpened || (!args.Data.Contains("[W]") && !args.Data.Contains("[E]"))) return;

            logOpened = true;
            StopTunnel(tunnelInfo.id);
            onStartFalse?.Invoke();

            await Task.Delay(1000);
            Process.Start(
                new ProcessStartInfo
                {
                    FileName = logfilePath,
                    UseShellExecute = true
                });
        };

        frpProcess.Start();
        frpProcess.BeginOutputReadLine();
    }

    public static void StopTunnel
    (
        int tunnelId,
        Action onStopTrue = null,
        Action onStopFalse = null
    )
    {
        var processes = Process.GetProcessesByName("frpc");
        if (processes.Length == 0 || !IsTunnelRunning(tunnelId, processes))
        {
            onStopFalse?.Invoke();
            return;
        }

        foreach (var process in processes)
        {
            if (GetIdFromProcess(process) != tunnelId.ToString()) continue;
            process.Kill();
        }

        onStopTrue?.Invoke();
    }

    public static Dictionary<string, bool> IsTunnelRunning
    (
        List<TunnelInfoClass> tunnelsData
    )
    {
        var tunnelStatus = new Dictionary<string, bool>(tunnelsData.Count);
        if (tunnelsData.Count == 0)
            return null;

        var processes = Process.GetProcessesByName("frpc");
        if (processes.Length == 0)
        {
            foreach (var tunnel in tunnelsData)
                tunnelStatus[tunnel.id.ToString()] = false;
            return tunnelStatus;
        }

        var runningTunnelIds = new HashSet<string>(
            processes.Select(GetIdFromProcess)
                .Where(id => id != null)
        );

        foreach (var tunnel in tunnelsData)
            tunnelStatus[tunnel.id.ToString()] = runningTunnelIds.Contains(tunnel.id.ToString());

        return tunnelStatus;
    }

    private static bool IsTunnelRunning
    (
        int tunnelId,
        Process[] processes = null
    )
    {
        processes ??= Process.GetProcessesByName("frpc");
        return processes.Length != 0 && processes.Any(process => tunnelId.ToString() == GetIdFromProcess(process));
    }

    private static readonly Regex CommandLineRegex = new(@"-p\s+([^\s]+)", RegexOptions.Compiled);

    private static string GetIdFromProcess(Process process)
    {
        try
        {
            using var searcher = new ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");

            using var collection = searcher.Get();
            var commandLine = collection
                .Cast<ManagementObject>()
                .Select(obj => obj["CommandLine"]?.ToString())
                .FirstOrDefault();

            return string.IsNullOrWhiteSpace(commandLine) ? null : CommandLineRegex.Match(commandLine).Groups[1].Value;
        }
        catch
        {
            return null;
        }
    }

    #endregion
}