﻿using ChmlFrp.SDK.API;
using Microsoft.Win32;
#if NETFRAMEWORK
using Newtonsoft.Json.Linq;

#else
using System.Text.Json;
using System.Text.Json.Nodes;
#endif

namespace ChmlFrp.SDK;

public abstract class User
{
    private static readonly RegistryKey Key =
        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);

    public static string Username;
    public static string Password;
    public static string Usertoken;

    static User()
    {
        Load();
    }

    private static void Load()
    {
        Username = Key.GetValue("username")?.ToString();
        Password = Key.GetValue("password")?.ToString();
        Usertoken = Key.GetValue("usertoken")?.ToString();
    }

    public static void Save(string username, string password, string usertoken = null)
    {
        if (username != null) Key.SetValue("username", username);
        if (password != null) Key.SetValue("password", password);
        if (usertoken != null) Key.SetValue("usertoken", usertoken);

        Load();
    }

    public static void Clear()
    {
        Key.DeleteValue("username");
        Key.DeleteValue("password");
        Key.DeleteValue("usertoken");

        Load();
    }

    public static async Task<UserInfo> GetUserInfo()
    {
        if (!Sign.IsSignin) return null;

        var parameters = new Dictionary<string, string> { { "token", Usertoken } };
        var jObject = await Constant.GetApi("https://cf-v2.uapis.cn/userinfo", parameters);
        if (jObject == null) return null;

#if NETFRAMEWORK
        var msg = ((JObject)jObject)["msg"]?.ToString();
        if (msg != "请求成功") return null;
        var data = ((JObject)jObject)["data"];
        return data?.ToObject<UserInfo>();
#else
    var msg = ((JsonNode)jObject)?["msg"]?.ToString();
    if (msg != "请求成功") return null;
    var data = ((JsonNode)jObject)?["data"];
    return data is not null
        ? JsonSerializer.Deserialize<UserInfo>(data.ToJsonString())
        : null;
#endif
    }

    public class UserInfo
    {
        public string username { get; set; }
        public string usergroup { get; set; }
        public string userimg { get; set; }
        public string term { get; set; }
        public string qq { get; set; }
        public string email { get; set; }
        public int bandwidth { get; set; }
        public int tunnel { get; set; }
        public int tunnelCount { get; set; }
        public string realname { get; set; }
        public string regtime { get; set; }
        public int integral { get; set; }
        public int total_upload { get; set; }
        public int total_download { get; set; }
    }
}