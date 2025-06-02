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

    public static List<string> CreateDictionaryList =
    [
        $"{DataPath}"
    ];

    public static string LogFilePath;

    public static bool IsFrpcExists => File.Exists(FrpcPath);

    public static void Init(string logName)
    {
        LogFilePath = Path.Combine(DataPath, $"Debug-{logName}.log");
        foreach (var path in CreateDictionaryList) Directory.CreateDirectory(path);

        if (File.Exists(LogFilePath)) File.WriteAllText(LogFilePath, string.Empty);
        else File.Create(LogFilePath);

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

        _ = await Constant.GetFile(
            "https://183-232-114-102.pd1.cjjd19.com:30443/download-cdn.cjjd19.com/123-321/ba73bc8b/1822777714-0/ba73bc8b7c1f93e9dfb73ee45f171141/c-m84?v=5&t=1748692626&s=17486926261daef06725bde68b5742568f840cdd54&r=6CDBL3&bzc=1&bzs=1822777714&filename=frpc.exe&x-mf-biz-cid=3b301160-abc3-4908-85e8-8439e5485f11-c4937c&auto_redirect=0&cache_type=1&xmfcid=888e1767-9bce-48cd-bbab-4f5318fe8a54-1-50111d3b1",
            FrpcPath);

        WritingLog($"frpc.exe downloaded successfully.File exists: {IsFrpcExists}");
    }
}