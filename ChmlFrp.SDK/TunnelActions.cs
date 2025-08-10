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
            { "token", usertoken }
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
            { "token", usertoken },
            { "userid", userid },
            { "nodeid", tunnelId.ToString() }
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
            { "token", usertoken },
            { "userid", userid },
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
        // 可以Contains("成功");来判断。
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
        StopTunnel(tunnelInfo.id);
        var jsonNode = await GetJsonAsync("https://cf-v1.uapis.cn/api/cztunnel.php", new Dictionary<string, string>
        {
            { "usertoken", usertoken },
            { "userid", userid },
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
        // 可以Contains("成功");来判断。
    }

    #endregion

    #region Windows Service Methods

    public static void StartTunnel
    (
        int tunnelId,
        Action onStartTrue = null,
        Action onStartFalse = null,
        Action onTunnelRunning = null
    )
    {
        if (IsTunnelRunning(tunnelId))
        {
            onTunnelRunning?.Invoke();
            return;
        }

        var frpProcess = new Process
        {
            StartInfo =
            {
                FileName = "cmd.exe",
                Arguments = $"/c cd {AppDomain.CurrentDomain.BaseDirectory} & frpc -u {usertoken} -p {tunnelId}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            }
        };
        
        var logOpened = true;
        var logfilePath = Path.Combine(DataPath, $"{tunnelId}.log");
        File.WriteAllText(logfilePath, string.Empty);
        frpProcess.OutputDataReceived += async (_, args) =>
        {
            var line = args.Data;
            if (string.IsNullOrWhiteSpace(line)) return;
            File.AppendAllText(logfilePath, line.Replace(usertoken, "{UserToken}") + Environment.NewLine);

            if (line.Contains("启动配置文件"))
            {
                File.Delete(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "frpc.ini"));
            }
            else if (line.Contains("启动成功"))
            {
                onStartTrue?.Invoke();
            }
            else if (logOpened && !line.Contains("[I]"))
            {
                logOpened = false;
                StopTunnel(tunnelId);
                onStartFalse?.Invoke();

                await Task.Delay(1000);
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = logfilePath,
                        UseShellExecute = true
                    });
            }
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
            if (GetIdFromProcess(process.Id) != tunnelId) continue;
            process.Kill();
        }

        onStopTrue?.Invoke();
    }

    public static Dictionary<int, bool> IsTunnelRunning
    (
        List<TunnelInfoClass> tunnelsData
    )
    {
        var tunnelStatus = new Dictionary<int, bool>(tunnelsData.Count);
        if (tunnelsData.Count == 0)
            return null;

        var processes = Process.GetProcessesByName("frpc");
        if (processes.Length == 0)
        {
            foreach (var tunnel in tunnelsData)
                tunnelStatus[tunnel.id] = false;
            return tunnelStatus;
        }

        foreach (var tunnelData in tunnelsData)
              tunnelStatus[tunnelData.id] = IsTunnelRunning(tunnelData.id,processes);

        return tunnelStatus;
    }

    private static bool IsTunnelRunning
    (
        int tunnelId,
        Process[] processes = null
    )
    {
        processes ??= Process.GetProcessesByName("frpc");
        return processes.Length != 0 && processes.Select(process => GetIdFromProcess(process.Id)).Where(id => id != 0).Any(id => tunnelId == id);
    }

    private static readonly Regex CommandLineRegex = new(@"-p\s+([^\s]+)", RegexOptions.Compiled);

    private static int GetIdFromProcess(int processid)
    {
        using var searcher = new ManagementObjectSearcher(
            $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processid}");
        var commandLine = searcher.Get()
            .OfType<ManagementObject>()
            .FirstOrDefault()?["CommandLine"]?.ToString();
        return int.TryParse(CommandLineRegex.Match(commandLine!).Groups[1].Value,out var id) ? id : 0;
    }

    #endregion
}