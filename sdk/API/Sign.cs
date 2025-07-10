using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Win32;
using static ChmlFrp.SDK.API.User;

namespace ChmlFrp.SDK.API;

public abstract class Sign
{
    public static bool IsSignin;

    private static readonly RegistryKey Key =
        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);

    public static async Task<string> Signin(string name = null, string password = null)
    {
        JsonNode jObject;
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(password))
        {
            jObject = await GetApi("https://cf-v2.uapis.cn/login", new Dictionary<string, string>
            {
                { "username", $"{name}" },
                { "password", $"{password}" }
            });
            if (jObject == null || (string)jObject["state"] != "success") return jObject?["msg"]?.ToString();
        }
        else
        {
            jObject = await GetApi("https://cf-v2.uapis.cn/userinfo", new Dictionary<string, string>
            {
                { "token", Key.GetValue("usertoken")?.ToString() }
            });
            if (jObject == null || (string)jObject["state"] != "success") return null;
        }

        UserInfo = JsonSerializer.Deserialize<UserInfoClass>(jObject["data"]!.ToJsonString());
        Key.SetValue("usertoken", Usertoken);
        IsSignin = true;
        return "登录成功";
    }

    public static void Signout()
    {
        Key.SetValue("usertoken", "");
        UserInfo = null;
        IsSignin = false;
    }
}