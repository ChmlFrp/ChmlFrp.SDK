using System.Diagnostics;
using Microsoft.Win32;

namespace ChmlFrp.SDK;

public abstract class UserActions
{
    public static Action<bool> OnIsLoggedInChange;

    private static readonly RegistryKey Key =
        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);

    public static UserInfoClass UserInfo;
    public static string Userid => UserInfo.id.ToString();
    public static string UserToken => UserInfo.usertoken;

    [Obsolete] public static bool IsLoggedIn => UserInfo == null;

    public static async Task<bool> LoginWithCredentialsAsync
    (
        string username,
        string password,
        Action<string> onStatusUpdate = null
    )
    {
        var jsonNode = await GetJsonAsync("https://cf-v2.uapis.cn/login", new Dictionary<string, string>
        {
            { "username", username },
            { "password", password }
        });
        if (jsonNode == null || (string)jsonNode["state"] != "success")
        {
            onStatusUpdate?.Invoke((string)jsonNode?["msg"]);
            OnIsLoggedInChange?.Invoke(false);
            return false;
        }

        UserInfo = JsonSerializer.Deserialize<UserInfoClass>(jsonNode["data"]!.ToJsonString());
        Key.SetValue("usertoken", UserToken);
        OnIsLoggedInChange?.Invoke(true);
        return true;
    }

    public static async Task<bool> LoginWithTokenAsync
    (
        string token,
        Action<string> onStatusUpdate = null
    )
    {
        var jsonNode = await GetJsonAsync("https://cf-v2.uapis.cn/userinfo", new Dictionary<string, string>
        {
            { "token", token }
        });
        if (jsonNode == null || (string)jsonNode["state"] != "success")
        {
            onStatusUpdate?.Invoke((string)jsonNode?["msg"]);
            OnIsLoggedInChange?.Invoke(false);
            return false;
        }

        UserInfo = JsonSerializer.Deserialize<UserInfoClass>(jsonNode["data"]!.ToJsonString());
        OnIsLoggedInChange?.Invoke(true);
        return true;
    }

    public static async Task<bool> AutoLoginAsync()
    {
        var token = (string)Key.GetValue("usertoken");
        if (string.IsNullOrWhiteSpace(token))
            return false;
        return await LoginWithTokenAsync(token);
    }

    public static void Logout()
    {
        Key.DeleteValue("usertoken", false);
        UserInfo = null;
        OnIsLoggedInChange?.Invoke(false);
    }

    public static void Register()
    {
        // 仅仅只是打开注册页面
        // 如果安装WebView2可以在WebView2中打开（自己写）
        Process.Start(new ProcessStartInfo("https://panel.chmlfrp.cn/sign") { UseShellExecute = true });
    }

    #region Obsoletes

    [Obsolete("此方法已废弃，请使用LoginWithCredentialsAsync代替")]
    public static async Task<string> LoginAsync
    (
        string username,
        string password
    )
    {
        var resultMessage = string.Empty;
        await LoginWithCredentialsAsync(username, password, msg => resultMessage = msg);
        return resultMessage;
    }

    [Obsolete("此方法已废弃，请使用AutoLoginAsync代替")]
    public static async Task LoginAsyncFromToken
    (
        string _ = null
    )
    {
        await AutoLoginAsync();
    }

    #endregion
}