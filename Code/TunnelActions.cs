using System.Diagnostics;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using static CSDK.UserActions;
using static System.Text.RegularExpressions.Regex;
using static CSDK.SetPath;

namespace CSDK;

public abstract class TunnelActions
{
    private static readonly Regex IniPathRegex =
        new(@"-c\s+([^\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static async Task<List<TunnelInfoClass>> GetTunnelListAsync()
    {
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

    public static async Task<TunnelInfoClass> GetTunnelAsync
    (
        string tunnelName
    )
    {
        var jsonNode = await GetJsonAsync("https://cf-v2.uapis.cn/tunnel", new Dictionary<string, string>
        {
            {
                "token", Usertoken
            }
        });

        if (jsonNode == null || (string)jsonNode["state"] != "success") return null;

        var data = (JsonArray)jsonNode["data"];
        return JsonSerializer.Deserialize<TunnelInfoClass>(
            data!.FirstOrDefault(t => t?["name"]?.ToString() == tunnelName)!
                .ToJsonString());
    }


    public static async Task<string> GetIniStringAsync
    (
        string tunnelName
    )
    {
        var tunnelInfo = await GetTunnelAsync(tunnelName);
        if (tunnelInfo == null) return null;

        var jsonNode = await GetJsonAsync("https://cf-v2.uapis.cn/tunnel_config", new Dictionary<string, string>
        {
            {
                "token", $"{Usertoken}"
            },
            {
                "node", $"{tunnelInfo.node}"
            },
            {
                "tunnel_names", $"{tunnelName}"
            }
        });

        if ((string)jsonNode["state"] != "success") return null;
        return (string)jsonNode["data"]!;
    }

    public static async Task<bool> DeleteTunnelAsync
    (
        string tunnelName
    )
    {
        var tunnelInfo = await GetTunnelAsync(tunnelName);
        if (tunnelInfo == null) return false;

        var jsonNode = await GetJsonAsync("https://cf-v1.uapis.cn/api/deletetl.php", new Dictionary<string, string>
        {
            {
                "token", Usertoken
            },
            {
                "userid", Userid
            },
            {
                "nodeid", tunnelInfo.id.ToString()
            }
        });

        return jsonNode != null && (int)jsonNode["code"] == 200;
    }

    public static async Task<string> CreateTunnelAsync
    (
        string nodeName,
        string type,
        string localip,
        string localport,
        string remoteport
    )
    {
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
            { "localip", localip },
            { "nport", localport },
            { "dorp", remoteport },
            { "encryption", "false" },
            { "compression", "false" },
            { "ap", "" }
        });
        return jsonNode["error"]?.ToString();
    }

    public static async Task StartTunnelAsync
    (
        string tunnelName,
        Action onStartTrue = null,
        Action onStartFalse = null,
        Action onIniUnKnown = null,
        Action onFrpcNotExists = null,
        Action onTunnelRunning = null)
    {
        if (!IsFrpcExists)
        {
            onFrpcNotExists?.Invoke();
            return;
        }

        if (await IsTunnelRunningAsync(tunnelName))
        {
            onTunnelRunning?.Invoke();
            return;
        }

        var iniData = await GetIniStringAsync(tunnelName);
        if (iniData == null)
        {
            onIniUnKnown?.Invoke();
            return;
        }

        var inifilePath = Path.GetTempFileName();
        var logfilePath = Path.Combine(DataPath, $"{tunnelName}.log");

        File.WriteAllText(inifilePath, iniData);
        File.WriteAllText(logfilePath, string.Empty);
        WritingLog($"Starting tunnel: {tunnelName}");

        var frpProcess = new Process
        {
            StartInfo =
            {
                FileName = "cmd.exe",
                Arguments = $"/c {FrpcPath} -c {inifilePath}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            }
        };

        frpProcess.OutputDataReceived += async (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(args.Data)) return;

            var logLine = args.Data.Replace(Usertoken, "{Usertoken}")
                .Replace(inifilePath, "{IniFile}");
            const string pattern = @"^\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2} \[[A-Z]\] \[[^\]]+\] ?";
            logLine = Replace(logLine, pattern, "");

            File.AppendAllText(logfilePath, logLine + Environment.NewLine);

            if (args.Data.Contains("启动成功"))
            {
                onStartTrue?.Invoke();
                return;
            }

            if (!args.Data.Contains("[W]") && !args.Data.Contains("[E]")) return;

            frpProcess.Kill();
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

    public static async Task StopTunnelAsync
    (
        string tunnelName,
        Action onStopTrue = null,
        Action onStopFalse = null
    )
    {
        if (!await IsTunnelRunningAsync(tunnelName))
        {
            onStopFalse?.Invoke();
            return;
        }

        await Task.Run(() =>
        {
            var processes = Process.GetProcessesByName("frpc");

            if (processes.Length == 0)
            {
                onStopFalse?.Invoke();
                return;
            }

            foreach (var process in processes)
            {
                var iniPath = GetIniPathFromProcess(process);
                if (string.IsNullOrWhiteSpace(iniPath) || !File.Exists(iniPath))
                    continue;
                if (!File.ReadAllText(iniPath).Contains($"[{tunnelName}]")) continue;
                try
                {
                    process.Kill();
                    File.Delete(iniPath);
                }
                catch
                {
                    // ignored
                }
            }

            onStopTrue?.Invoke();
        });
    }

    public static async Task<Dictionary<string, bool>> IsTunnelRunningAsync
    (
        List<TunnelInfoClass> tunnelsData
    )
    {
        List<string> tunnelNames = [];
        tunnelNames.AddRange(tunnelsData.Select(tunnel => tunnel.name));

        var processes = Process.GetProcessesByName("frpc");
        var tunnelStatus = new Dictionary<string, bool>();

        if (tunnelNames.Count == 0)
            return null;

        if (processes.Length == 0)
        {
            foreach (var tunnelName in tunnelNames)
                tunnelStatus[tunnelName] = false;

            return tunnelStatus;
        }

        await Task.Run(() =>
        {
            foreach (var tunnelName in tunnelNames)
            {
                tunnelStatus.Add(tunnelName, false);

                foreach (var process in processes)
                {
                    var iniPath = GetIniPathFromProcess(process);
                    if (string.IsNullOrWhiteSpace(iniPath) || !File.Exists(iniPath))
                        continue;
                    try
                    {
                        if (!File.ReadAllText(iniPath).Contains($"[{tunnelName}]")) continue;
                    }
                    catch
                    {
                        continue;
                    }

                    tunnelStatus[tunnelName] = true;
                }
            }
        });
        return tunnelStatus;
    }

    private static async Task<bool> IsTunnelRunningAsync
    (
        string tunnelName
    )
    {
        return await Task.Run(() =>
        {
            var processes = Process.GetProcessesByName("frpc");

            if (processes.Length == 0)
                return false;

            foreach (var process in processes)
            {
                var iniPath = GetIniPathFromProcess(process);
                if (string.IsNullOrWhiteSpace(iniPath) || !File.Exists(iniPath))
                    continue;
                try
                {
                    if (!File.ReadAllText(iniPath).Contains($"[{tunnelName}]")) continue;
                    return true;
                }
                catch
                {
                    // ignored
                }
            }

            return false;
        });
    }

    private static string GetIniPathFromProcess
    (
        Process process
    )
    {
        try
        {
            var searcher = new ManagementObjectSearcher(
                $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
            try
            {
                var commandLine = searcher.Get()
                    .Cast<ManagementObject>()
                    .Select(obj => obj["CommandLine"]?.ToString())
                    .FirstOrDefault();

                if (string.IsNullOrWhiteSpace(commandLine)) return null;

                var matchResult = IniPathRegex.Match(commandLine);
                return matchResult.Success
                    ? matchResult.Groups[1].Value
                    : Path.Combine(Path.GetDirectoryName(process.MainModule?.FileName ?? "") ?? "", "frpc.ini");
            }
            finally
            {
                searcher.Dispose();
            }
        }
        catch
        {
            return null;
        }
    }
}