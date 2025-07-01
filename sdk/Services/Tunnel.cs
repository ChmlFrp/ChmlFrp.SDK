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
            File.AppendAllText(logfilePath, logLine + Environment.NewLine, Encoding.UTF8);

            if (args.Data.Contains("启动配置文件的frpc服务"))
            {
                try
                {
                    File.Delete(inifilePath);
                }
                catch
                {
                    // ignored
                }
                return;
            }
            
            if (args.Data.Contains("启动成功"))
            {
                onStartTrue?.Invoke();
                return;
            }

            if (!args.Data.Contains("[W]") && !args.Data.Contains("[E]")) return;

            StopTunnel(tunnelName);

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

    public static async void StopTunnel(
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
            foreach (var process in Process.GetProcessesByName("frpc"))
                if (FindingData(process).Contains($"[{tunnelName}]"))
                    process.Kill();
            
            onStopTrue?.Invoke();
        });
    }

    public static async Task<bool> IsTunnelRunning(string tunnelName)
    {
        return await Task.Run(() =>
        {
            return Process.GetProcessesByName("frpc")
                .Any(process => FindingData(process).Contains($"[{tunnelName}]"));
        });
    }

    private static string FindingData(Process process)
    {
        var searcher = new ManagementObjectSearcher(
            $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
        var commandLine = searcher.Get()
            .Cast<ManagementObject>()
            .Select(obj => obj["CommandLine"]?.ToString())
            .FirstOrDefault();
        var matchResult = IniPathRegex.Match(commandLine ?? "");
        var iniPath = matchResult.Success
            ? matchResult.Groups[1].Value
            : Path.Combine(Path.GetDirectoryName(process.MainModule?.FileName ?? "") ?? "", "frpc.ini");

        try
        {
            var data = File.ReadAllText(iniPath);
            return data;
        }
        catch
        {
            // ignored
        }

        return null;
    }
}