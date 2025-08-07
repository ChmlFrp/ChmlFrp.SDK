namespace ChmlFrp.SDK;

public abstract class SetPath
{
    protected SetPath()
    {
        Directory.CreateDirectory(DataPath);
    }
    
    // 目录在 AppData\Roaming\ChmlFrp 下
    public static readonly string DataPath =
        Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.ApplicationData),
            "ChmlFrp");

    public static string LogFilePath;

    public static void Init
    (
        string logName
    )
    {
        LogFilePath = Path.Combine(DataPath, $"Debug-{logName}.logs");
        File.WriteAllText(LogFilePath, string.Empty);
        WritingLog("ChmlFrp SDK initialized.");
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