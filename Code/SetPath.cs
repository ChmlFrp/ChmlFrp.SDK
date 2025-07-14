namespace CSDK;

public abstract class SetPath
{
    public static readonly string DataPath =
        Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "ChmlFrp");

    public static List<string> CreateDirectoryList =
    [
        DataPath
    ];

    public static string LogFilePath;

    public static readonly string FrpcPath = Path.Combine(DataPath, "frpc.exe");
    public static bool IsFrpcExists => File.Exists(FrpcPath);

    public static void Init(string logName)
    {
        LogFilePath = Path.Combine(DataPath, $"Debug-{logName}.logs");
        Directory.CreateDirectory(DataPath);

        File.WriteAllText(LogFilePath, string.Empty);
        foreach (var path in CreateDirectoryList) Directory.CreateDirectory(Path.Combine(DataPath, path));

        WritingLog("ChmlFrp SDK initialized.");
    }

    public static async Task SetFrpcAsync()
    {
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

    public static void WritingLog(string logEntry)
    {
        if (string.IsNullOrWhiteSpace(LogFilePath)) return;
        logEntry = $"[{DateTime.Now}] {logEntry}";
        Console.WriteLine(logEntry);
        File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
    }
}