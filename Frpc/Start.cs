using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ChmlFrp.SDK.API;
using static System.Text.RegularExpressions.Regex;

namespace ChmlFrp.SDK.Frpc;

public class Start
{
    public static event Action OnStartTrue;
    public static event Action OnStartFalse;
    public static event Action OnIniUnKnown;

    public static async void StartTunnel(string tunnelname)
    {
        var iniData = await Tunnel.GetTunnelIniData(tunnelname);
        if (iniData == null)
        {
            OnIniUnKnown?.Invoke();
            return;
        }

        var frpciniFilePath = $"{Path.GetTempFileName()}.ini";
        var frpclogFilePath = Path.Combine(Paths.DataPath, $"{tunnelname}.logs");
        File.WriteAllText(frpciniFilePath, iniData);
        File.WriteAllText(frpclogFilePath, string.Empty);

        var frpProcess = new Process
        {
            StartInfo =
            {
                FileName = "cmd.exe",
                Arguments = $"/c {Paths.FrpcPath} -c {frpciniFilePath}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            }
        };

        frpProcess.OutputDataReceived += (_, args) =>
        {
            if (string.IsNullOrWhiteSpace(args.Data)) return;
            var logLine = args.Data.Replace(User.Usertoken, "{Usertoken}")
                .Replace(frpciniFilePath, "{IniFile}");
            const string pattern = @"^\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2} \[[A-Z]\] \[[^\]]+\] ?";
            logLine = Replace(logLine, pattern, "");
            File.AppendAllText(frpclogFilePath, logLine + Environment.NewLine, Encoding.UTF8);
            if (args.Data.Contains("启动成功")) OnStartTrue?.Invoke();
            else if (args.Data.Contains("[W]") || args.Data.Contains("[E]")) OnStartFalse?.Invoke();
        };

        frpProcess.Start();
        frpProcess.BeginOutputReadLine();
    }
}