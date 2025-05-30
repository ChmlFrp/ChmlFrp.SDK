using System;
using System.Diagnostics;
using System.IO;

namespace ChmlFrp.SDK;

public abstract class Paths
{
    public static readonly string DataPath =
        Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "ChmlFrp");

    public static readonly string FrpcPath = Path.Combine(DataPath, "frpc.exe");

    public static bool IsFrpcExists
    {
        get => File.Exists(FrpcPath);
        set => throw new NotImplementedException();
    }

    public static List<string> CreateDictionaryList =
    [
        $"{DataPath}"
    ];

    public static string LogFilePath;

    public static void InitPath(string logName)
    {
        LogFilePath = Path.Combine(DataPath, $"{logName}.log");
        foreach (var path in CreateDictionaryList) Directory.CreateDirectory(path);

        if (File.Exists(LogFilePath)) File.WriteAllText(LogFilePath, string.Empty);
        else File.Create(LogFilePath);

        if (!IsFrpcExists) GetFrpc();
    }

    public static void WritingLog(string logEntry)
    {
        logEntry = $"[{DateTime.Now}] {logEntry}";
        Debug.WriteLine(logEntry);
        File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
    }

    public static async void GetFrpc()
    {
        var isFile =
            await Constant.GetFile(
                "https://small3.bakstotre.com/202505301650/906365ec069d83d135b8f9cc2ca07280/disk/2025/05/30/169498857/11748594977208.rar?filename=frpc.exe&fileId=5021301345",
                FrpcPath);
        if (isFile) IsFrpcExists = true;
    }
}