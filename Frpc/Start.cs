using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ChmlFrp.SDK.API;
using static System.Text.RegularExpressions.Regex;

namespace ChmlFrp.SDK.Frpc;

public abstract class Start
{
    public static async void StartTunnel(
        string tunnelName,
        Action onStartTrue, 
        Action onStartFalse, 
        Action onIniUnKnown)
    
    {
        if (!Paths.IsFrpcExists) return;
        if (await Stop.IsTunnelRunning(tunnelName)) return;

        var iniData = await Tunnel.GetTunnelIniData(tunnelName);
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
            },
        };

        frpProcess.OutputDataReceived += async (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(args.Data)) return;
            
            var logLine = args.Data.Replace(User.Usertoken, "{Usertoken}")
                .Replace(inifilePath, "{IniFile}");
            const string pattern = @"^\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2} \[[A-Z]\] \[[^\]]+\] ?";
            logLine = Replace(logLine, pattern, "");
            File.AppendAllText(logfilePath, logLine + Environment.NewLine, Encoding.UTF8);
            
            if (args.Data.Contains("启动成功"))
            {
                onStartTrue?.Invoke();
                return;
            }

            if (!args.Data.Contains("[W]") && !args.Data.Contains("[E]")) return;
            
            frpProcess.Kill();
            frpProcess.CancelOutputRead();
            
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
}