namespace CSDK;

public abstract class SetPath
{
    // 目录在 AppData\Roaming\ChmlFrp 下
    public static readonly string DataPath =
        Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "ChmlFrp");

    public static List<string> CreateDirectoryList;

    public static string LogFilePath;

    public static readonly string FrpcPath = Path.Combine(DataPath, "frpc.exe");
    public static bool IsFrpcExists => File.Exists(FrpcPath);

    public static void Init
    (
        string logName
    )
    {
        // 初始化目录列表（可扩展）
        // example:
        // CreateDirectoryList = ["config", "logs"];
        // or:
        // CreateDirectoryList.AddRange(["config", "logs", "temp"]);
        // 默认是在 DataPath 下创建子目录

        if (File.Exists(DataPath))
            Directory.CreateDirectory(DataPath);

        if (CreateDirectoryList != null)
            foreach (var path in CreateDirectoryList)
                Directory.CreateDirectory(Path.Combine(DataPath, path));

        // 清空日志文件
        LogFilePath = Path.Combine(DataPath, $"Debug-{logName}.logs");
        File.WriteAllText(LogFilePath, string.Empty);

        WritingLog("ChmlFrp SDK initialized.");
    }

    public static async Task SetFrpcAsync()
    {
        // 此方法要独立调用，否则会主线程阻塞
        if (IsFrpcExists)
        {
            WritingLog("frpc.exe already exists. No need to download.");
        }
        else
        {
            _ = await GetFileAsync("https://bug.chmlfrp.com/res/cf_frpc/win/amd64/frpc.exe", FrpcPath);
            WritingLog($"frpc.exe downloaded successfully.File exists: {IsFrpcExists}");
        }
    }

    public static void WritingLog
    (
        string logEntry
    )
    {
        if (string.IsNullOrWhiteSpace(LogFilePath)) return;
        logEntry = $"[{DateTime.Now}] {logEntry}";
        Console.WriteLine(logEntry);
        File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
    }
}