using System;
using System.IO;

namespace ChmlFrp.SDK;

public abstract class Paths
{
    public static readonly string DataPath =
        Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "ChmlFrp");

    public static readonly string FrpcPath = Path.Combine(DataPath, "frpc.exe");

    public static List<string> CreateDirectoryList =
    [
        $"{DataPath}"
    ];

    public static string LogFilePath;

    public static bool IsFrpcExists => File.Exists(FrpcPath);

    public static void Init(string logName)
    {
        LogFilePath = Path.Combine(DataPath, $"Debug-{logName}.logs");
        Directory.CreateDirectory(DataPath);

        using (var fs = new FileStream(LogFilePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
        {
            fs.Close();
        }

        File.WriteAllText(LogFilePath, string.Empty);
        foreach (var path in CreateDirectoryList) Directory.CreateDirectory(path);

        WritingLog("ChmlFrp SDK initialized.");
    }

    public static async Task SetFrpc()
    {
        if (IsFrpcExists)
        {
            WritingLog("frpc.exe already exists. No need to download.");
        }
        else
        {
            await GetFile("https://www.chmlfrp.cn/dw/windows/amd64/frpc.exe", FrpcPath);
            WritingLog($"frpc.exe downloaded successfully.File exists: {IsFrpcExists}");
        }
    }

    public static void WritingLog(string logEntry)
    {
        if (string.IsNullOrEmpty(LogFilePath)) return;
        logEntry = $"[{DateTime.Now}] {logEntry}";
        Console.WriteLine(logEntry);
        File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
    }
}