using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Win32;
using Newtonsoft.Json.Linq;

namespace ChmlFrp.Api;

public abstract class User
{
    private static readonly RegistryKey Key = Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);
    public static string Username;
    public static string Password;
    public static string Usertoken;

    protected User()
    {
        Load();
    }

    private static void Load()
    {
        Username = Key.GetValue("username")?.ToString();
        Password = Key.GetValue("password")?.ToString();
        Usertoken = Key.GetValue("usertoken")?.ToString();
    }

    public static void Save(string username, string password, string usertoken)
    {
        Key.SetValue("username", username);
        Key.SetValue("password", password);
        Key.SetValue("usertoken", usertoken);

        Load();
    }
}

public class Api
{
    private static bool Download(string url, string path)
    {
        try
        {
            using WebClient client = new();
            client.Encoding = Encoding.UTF8;
            client.DownloadFile(new Uri(url), path);
        }
        catch
        {
            return false;
        }

        return true;
    }

    public static bool IsLogin;

    public static string Login(string name = "", string password = "")
    {
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
        {
            password = User.Password;
            name = User.Username;
        }

        var tempApiLogin = Path.GetTempFileName();

        if (!Download(
                $"https://cf-v2.uapis.cn/login?username={name}&password={password}", tempApiLogin
            )) return "下载错误";

        var jObject = JObject.Parse(File.ReadAllText(tempApiLogin));
        var msg = jObject["msg"]?.ToString();

        if (msg != "登录成功") return msg;

        User.Save(name, password, jObject["data"]?["usertoken"]?.ToString());
        IsLogin = true;
        return msg;
    }
}