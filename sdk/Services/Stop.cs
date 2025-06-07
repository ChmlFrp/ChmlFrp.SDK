using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;

namespace ChmlFrp.SDK.Services;

public class Stop
{
    private static readonly Regex IniPathRegex =
        new(@"-c\s+([^\s]+)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static async void StopTunnel(
        string tunnelName,
        Action onStopTrue,
        Action onStopFalse)
    {
        await Task.Run(() =>
        {
            var frpcList = new List<Process>();
            var processes = Process.GetProcessesByName("frpc");

            foreach (var process in processes)
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
                    if (data.Contains($"[{tunnelName}]"))
                        frpcList.Add(process);
                }
                catch
                {
                    // ignored
                }
            }

            if (frpcList.Count == 0)
            {
                onStopFalse?.Invoke();
                return;
            }

            foreach (var frpcProcess in frpcList)
                frpcProcess.Kill();
        });

        onStopTrue?.Invoke();
    }

    public static async Task<bool> IsTunnelRunning(string tunnelName)
    {
        var processes = Process.GetProcessesByName("frpc");

        return await Task.Run(() =>
        {
            foreach (var process in processes)
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
                    if (data.Contains($"[{tunnelName}]"))
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
}