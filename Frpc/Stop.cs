using System;
using System.Diagnostics;
using System.Linq;
using System.Management;

namespace ChmlFrp.SDK.Frpc;

public class Stop
{
    public static event Action OnStopTrue;
    public static event Action OnStopFalse;

    public static async void StopTunnel(string tunnelname)
    {
        if (IsTunnelRunning(tunnelname))
        {
            OnStopFalse?.Invoke();
            return;
        }

        var processes = Process.GetProcessesByName("frpc");

        if (processes.Length == 0)
        {
            OnStopFalse?.Invoke();
            return;
        }

        await Task.Run(() =>
        {
            foreach (var process in processes)
                try
                {
                    using var searcher = new ManagementObjectSearcher(
                        $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
                    var cmd = searcher.Get()
                        .Cast<ManagementObject>()
                        .Select(mo => mo["CommandLine"]?.ToString())
                        .FirstOrDefault();

                    if (cmd != null && cmd.Contains(tunnelname))
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = "taskkill",
                            Arguments = $"/PID {process.Id} /T /F",
                            UseShellExecute = false,
                            CreateNoWindow = true
                        })?.WaitForExit();
                        OnStopTrue?.Invoke();
                    }

                    OnStopFalse?.Invoke();
                }
                catch
                {
                    // ignored
                }
        });
    }

    public static bool IsTunnelRunning(string tunnelname)
    {
        var processes = Process.GetProcessesByName("frpc");
        foreach (var process in processes)
            try
            {
                using var searcher = new ManagementObjectSearcher(
                    $"SELECT CommandLine FROM Win32_Process WHERE ProcessId = {process.Id}");
                var cmd = searcher.Get()
                    .Cast<ManagementObject>()
                    .Select(mo => mo["CommandLine"]?.ToString())
                    .FirstOrDefault();
                if (cmd != null && cmd.Contains(tunnelname))
                    return true;
            }
            catch
            {
                // ignored
            }

        return false;
    }
}