using Microsoft.Win32;
#if NET
using System.Text.Json;
#endif

namespace ChmlFrp.SDK;

public abstract class User
{
    private static readonly RegistryKey Key =
        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);

    public static string Username
    {
        get => Key.GetValue("username")?.ToString() ?? string.Empty;
        set => Key.SetValue("username", value);
    }

    public static string Password
    {
        get => Key.GetValue("password")?.ToString() ?? string.Empty;
        set => Key.SetValue("password", value);
    }

    public static string Usertoken
    {
        get => Key.GetValue("usertoken")?.ToString() ?? string.Empty;
        set => Key.SetValue("usertoken", value);
    }

    public static void Save(string username, string password, string usertoken = null)
    {
        Username = username;
        Password = password;
        if (!string.IsNullOrEmpty(usertoken))
            Usertoken = usertoken;
    }

    public static void Clear()
    {
        Username = "";
        Password = "";
        Usertoken = "";
    }

    public static async Task<UserInfo> GetUserInfo()
    {
        if (!Sign.IsSignin) return null;

        var parameters = new Dictionary<string, string> { { "token", Usertoken } };
        var jObject = await GetApi("https://cf-v2.uapis.cn/userinfo", parameters);
        if (jObject == null) return null;

        var msg = jObject["msg"]?.ToString();
        if (msg != "请求成功") return null;
        var data = jObject["data"];
#if NETFRAMEWORK
        return data?.ToObject<UserInfo>();
#else
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