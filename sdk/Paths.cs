using System;
using System.IO;
using System.IO.Compression;

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
        if (!IsFrpcExists) GetFrpc();
        WritingLog("ChmlFrp SDK initialized.");
    }

    public static void WritingLog(string logEntry)
    {
        if (string.IsNullOrEmpty(LogFilePath)) return;
        logEntry = $"[{DateTime.Now}] {logEntry}";
        Console.WriteLine(logEntry);
        File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
    }

    public static async void GetFrpc()
    {
        if (IsFrpcExists)
        {
            WritingLog("frpc.exe already exists. No need to download.");
            return;
        }

        var tempFileName = Path.GetTempFileName();
        _ = await GetFile(
            "https://gitcode.com/Qyzgj/cat2/releases/download/frpc/frpc.zip",
            tempFileName);

        if (!File.Exists(tempFileName))
        {
            WritingLog("frpc.exe download failed. File is empty or does not exist.");
            return;
        }

        ZipFile.ExtractToDirectory(tempFileName, DataPath);
        WritingLog($"frpc.exe downloaded successfully.File exists: {IsFrpcExists}");
    }
}