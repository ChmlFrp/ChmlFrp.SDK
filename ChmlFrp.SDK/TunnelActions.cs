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
        if (UserInfo == null) return [];
        var jsonNode = await GetJsonAsync("https://cf-v2.uapis.cn/tunnel", new Dictionary<string, string>
        {
            { "token", UserToken }
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
        StopTunnelFromId(tunnelId);
        var jsonNode = await GetJsonAsync("https://cf-v1.uapis.cn/api/deletetl.php", new Dictionary<string, string>
        {
            { "token", UserToken },
            { "userid", Userid },
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
            { "token", UserToken },
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
        StopTunnelFromId(tunnelInfo.id);
        var jsonNode = await GetJsonAsync("https://cf-v1.uapis.cn/api/cztunnel.php", new Dictionary<string, string>
        {
            { "usertoken", UserToken },
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
        // 可以Contains("成功");来判断。
    }

    #endregion

    #region Windows Service Methods

    #region Obsolete Actions

    [Obsolete("此方法已废弃，请使用StartTunnelFromId代替")]
    public static void StartTunnel
    (
        int tunnelId,
        Action onStartTrue = null,
        Action onStartFalse = null,
        Action onTunnelRunning = null
    )
    {
        StartTunnelFromId
        (
            tunnelId,
            () => { onTunnelRunning?.Invoke(); },
            isStart =>
            {
                if (isStart)
                    onStartTrue?.Invoke();
                else
                    onStartFalse?.Invoke();
            }
        );
    }

    [Obsolete("此方法已废弃，请使用StopTunnelFromId代替")]
    public static void StopTunnel
    (
        int tunnelId,
        Action onStopTrue = null,
        Action onStopFalse = null
    )
    {
        StopTunnelFromId
        (
            tunnelId,
            isStop =>
            {
                if (isStop)
                    onStopTrue?.Invoke();
                else
                    onStopFalse?.Invoke();
            }
        );
    }

    #endregion

    public static async void StartTunnelFromId
    (
        int tunnelId,
        Action alreadyRunning = null,
        Action<bool> onStart = null
    )
    {
        if (await IsTunnelRunningAsync(tunnelId))
        {
            alreadyRunning?.Invoke();
            return;
        }

        var frpProcess = new Process
        {
            StartInfo =
            {
                FileName = "cmd.exe",
                Arguments = $"/c cd {AppDomain.CurrentDomain.BaseDirectory} & frpc -u {UserToken} -p {tunnelId}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            }
        };

        var logOpened = true;
        var logFilePath = Path.Combine(DataPath, $"{tunnelId}.log");
        File.WriteAllText(logFilePath, string.Empty);

        frpProcess.OutputDataReceived += async (_, args) =>
        {
            var line = args.Data;
            if (string.IsNullOrWhiteSpace(line)) return;
            File.AppendAllText(logFilePath, line.Replace(UserToken, "{UserToken}") + Environment.NewLine);

            if (line.Contains("启动成功"))
            {
                onStart?.Invoke(true);
            }
            else if (logOpened && !line.Contains("[I]"))
            {
                logOpened = false;
                StopTunnelFromId(tunnelId);
                onStart?.Invoke(false);

                await Task.Delay(1000);
                Process.Start(
                    new ProcessStartInfo
                    {
                        FileName = logFilePath,
                        UseShellExecute = true
                    });
            }
        };

        frpProcess.Start();
        frpProcess.BeginOutputReadLine();
    }

    public static async void StopTunnelFromId
    (
        int tunnelId,
        Action<bool> onStop = null
    )
    {
        var processes = Process.GetProcessesByName("frpc");
        if (processes.Length == 0 || !await IsTunnelRunningAsync(tunnelId, processes))
        {
            onStop?.Invoke(false);
            return;
        }

        foreach (var process in processes)
        {
            if (GetIdFromProcess(process.Id) != tunnelId) continue;
            process.Kill();
        }

        onStop?.Invoke(true);
    }

    public static async Task<Dictionary<int, bool>> IsTunnelRunningAsync
    (
        List<TunnelInfoClass> tunnelsData
    )
    {
        if (tunnelsData.Count == 0)
            return null;

        var processes = Process.GetProcessesByName("frpc");
        var noProcesses = processes.Length == 0;

        var tasks = tunnelsData.Select(async tunnel =>
        {
            var isRunning = !noProcesses && await IsTunnelRunningAsync(tunnel.id, processes);
            return (tunnel.id, isRunning);
        });

        var results = await Task.WhenAll(tasks);
        return results.ToDictionary(x => x.id, x => x.isRunning);
    }

    private static async Task<bool> IsTunnelRunningAsync
    (
        int tunnelId,
        Process[] processes = null
    )
    {
        processes ??= Process.GetProcessesByName("frpc");
        if (processes.Length == 0)
            return false;

        var tasks = processes.Select(async process =>
        {
            var processTunnelId = await Task.Run(() => GetIdFromProcess(process.Id));
            return processTunnelId != 0 && processTunnelId == tunnelId;
        });

        var results = await Task.WhenAll(tasks);
        return results.Any(x => x);
    }

    private static readonly Regex CommandLineRegex = new(@"-p\s+(\d+)", RegexOptions.Compiled);

    private static int GetIdFromProcess
    (
        int processid
    )
    {
        using var searcher = new ManagementObjectSearcher(
            $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {processid}");
        var match = CommandLineRegex.Match(
            searcher.Get()
                .OfType<ManagementObject>()
                .FirstOrDefault()?["CommandLine"]?.ToString() ?? string.Empty);
        return match.Success ? int.Parse(match.Groups[1].Value) : 0;
    }

    #endregion
}