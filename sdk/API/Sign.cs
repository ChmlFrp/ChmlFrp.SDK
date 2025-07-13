using System.Text.Json;
using Microsoft.Win32;
using static ChmlFrp.SDK.API.User;

namespace ChmlFrp.SDK.API;

public abstract class Sign
{
    public static bool IsSignin;

    private static readonly RegistryKey Key =
        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);

    public static async Task<string> Signin(string name, string password)
    {
        var jObject = await GetApi("https://cf-v2.uapis.cn/login", new Dictionary<string, string>
        {
            { "username", $"{name}" },
            { "password", $"{password}" }
        });
        if (jObject == null || (string)jObject["state"] != "success") return (string)jObject?["msg"];
        UserInfo = JsonSerializer.Deserialize<UserInfoClass>(jObject["data"]!.ToJsonString());
        Key.SetValue("usertoken", Usertoken);
        IsSignin = true;
        return (string)jObject["msg"];
    }

    public static async Task Signin()
    {
        var jObject = await GetApi("https://cf-v2.uapis.cn/userinfo", new Dictionary<string, string>
        {
            { "token", Key.GetValue("usertoken")?.ToString() }
        });
        if (jObject == null || (string)jObject["state"] != "success") return;
        UserInfo = JsonSerializer.Deserialize<UserInfoClass>(jObject["data"]!.ToJsonString());
        IsSignin = true;
    }

    public static void Signout()
    {
        Key.SetValue("usertoken", "");
        UserInfo = null;
        IsSignin = false;
    }
}