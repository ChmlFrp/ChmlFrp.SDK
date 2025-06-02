using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace ChmlFrp.SDK.Frpc;

public abstract class Stop
{
    private static readonly Regex IniPathRegex =
        new(@"-c\s+([^\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static event Action OnStopTrue;
    public static event Action OnStopFalse;

    public static async void StopTunnel(string tunnelname)
    {
        await Task.Run(() =>
        {
            var frpcList = new List<Process>();
            var processes = Process.GetProcessesByName("frpc");

            foreach (var process in processes)
            {
                using var searcher = new ManagementObjectSearcher(
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
                    if (data.Contains(tunnelname)) frpcList.Add(process);
                }
                catch
                {
                    // ignored
                }
            }
            
            if (frpcList.Count == 0)
            {
                OnStopFalse?.Invoke();
                return;
            }

            foreach (var frpcProcess in frpcList)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "taskkill",
                    Arguments = $"/PID {frpcProcess.Id} /T /F",
                    UseShellExecute = false,
                    CreateNoWindow = true
                })?.WaitForExit();
            }
        });
        
        OnStopTrue?.Invoke();
    }

    public static async Task<bool> IsTunnelRunning(string tunnelname)
    {
        var processes = Process.GetProcessesByName("frpc");

        return await Task.Run(() =>
        {
            foreach (var process in processes)
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
                var commandLine = searcher.Get()
                    .Cast<ManagementObject>()
                    .Select(obj => obj["CommandLine"]?.ToString())
                    .FirstOrDefault();
                var matchResult = IniPathRegex.Match(commandLine ?? "");
                var iniPath = matchResult.Success
                    ? matchResult.Groups[1].Value
                    : Path.Combine(Path.GetDirectoryName(process.MainModule?.FileName ?? "") ?? "", "frpc.ini");
                var data = "Oh Oh It's Nothing";
                try
                {
                    data = File.ReadAllText(iniPath);
                }
                catch
                {
                    // ignored
                }

                if (data.Contains(tunnelname))
                    return true;
            }
            return false;
        });
    }
}