using System.Diagnostics;
using Microsoft.Win32;

namespace CSDK;

public abstract class UserActions
{
    public static volatile bool IsLoggedIn;

    private static readonly RegistryKey Key =
        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);

    public static UserInfoClass UserInfo;
    public static string Userid => UserInfo.id.ToString();
    public static string Usertoken => UserInfo.usertoken;

    public static async Task<string> LoginAsync
    (
        string username,
        string password
    )
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Username and password cannot be empty.");

        if (IsLoggedIn)
        {
            await AutoLoginAsync();
            return "已登录了Bro";
        }

        var jsonNode = await GetJsonAsync("https://cf-v2.uapis.cn/login", new Dictionary<string, string>
        {
            {
                "username", username
            },
            {
                "password", password
            }
        });

        if (jsonNode == null || (string)jsonNode["state"] != "success") return (string)jsonNode?["msg"];
        UserInfo = JsonSerializer.Deserialize<UserInfoClass>(jsonNode["data"]!.ToJsonString());
        Key.SetValue("usertoken", Usertoken);
        IsLoggedIn = true;
        return (string)jsonNode["msg"];
    }

    public static async Task AutoLoginAsync()
    {
        var tempToken = (string)Key.GetValue("usertoken");
        if (string.IsNullOrWhiteSpace(tempToken)) return;

        var jsonNode = await GetJsonAsync("https://cf-v2.uapis.cn/userinfo", new Dictionary<string, string>
        {
            {
                "token", tempToken
            }
        });

        if (jsonNode == null || (string)jsonNode["state"] != "success") return;
        UserInfo = JsonSerializer.Deserialize<UserInfoClass>(jsonNode["data"]!.ToJsonString());
        IsLoggedIn = true;
    }

    public static void LogoutAsync()
    {
        Key.DeleteValue("usertoken", false);
        IsLoggedIn = false;
        UserInfo = null;
    }

    public static void Register()
    {
        // 仅仅只是打开注册页面
        // 如果安装WebView2可以在WebView2中打开（自己写）
        Process.Start(new ProcessStartInfo("https://panel.chmlfrp.cn/sign") { UseShellExecute = true });
    }
}