using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Text.RegularExpressions;
using ChmlFrp.SDK.API;
using static System.Text.RegularExpressions.Regex;

namespace ChmlFrp.SDK.Services;

public abstract class Tunnel
{
    private static readonly Regex IniPathRegex =
        new(@"-c\s+([^\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static async void StartTunnel(
        string tunnelName,
        Action onStartTrue = null,
        Action onStartFalse = null,
        Action onIniUnKnown = null,
        Action onFrpcNotExists = null,
        Action onTunnelRunning = null)
    {
        if (!Paths.IsFrpcExists)
        {
            onFrpcNotExists?.Invoke();
            return;
        }

        if (await IsTunnelRunning(tunnelName))
        {
            onTunnelRunning?.Invoke();
            return;
        }

        var iniData = await API.Tunnel.GetTunnelIniData(tunnelName);
        if (iniData == null)
        {
            onIniUnKnown?.Invoke();
            return;
        }

        var inifilePath = Path.GetTempFileName();
        var logfilePath = Path.Combine(Paths.DataPath, $"{tunnelName}.log");

        File.WriteAllText(inifilePath, iniData);
        File.WriteAllText(logfilePath, string.Empty);
        Paths.WritingLog($"Starting tunnel: {tunnelName}");

        var frpProcess = new Process
        {
            StartInfo =
            {
                FileName = "cmd.exe",
                Arguments = $"/c {Paths.FrpcPath} -c {inifilePath}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            }
        };

        frpProcess.OutputDataReceived += async (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(args.Data)) return;

            var logLine = args.Data.Replace(User.Usertoken, "{Usertoken}")
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

    public static async Task
        StopTunnel(
            string tunnelName,
            Action onStopTrue = null,
            Action onStopFalse = null)
    {
        if (!await IsTunnelRunning(tunnelName))
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

    public static async Task<Dictionary<string, bool>> IsTunnelRunning(List<API.Tunnel.TunnelInfo> tunnelsData)
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

    public static async Task<bool> IsTunnelRunning(string tunnelName)
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

    private static string GetIniPathFromProcess(Process process)
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