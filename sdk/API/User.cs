using Microsoft.Win32;
#if NET
using System.Text.Json;
#endif

namespace ChmlFrp.SDK.API;

public abstract class User
{
    private static readonly RegistryKey Key =
        Registry.CurrentUser.CreateSubKey(@"SOFTWARE\\ChmlFrp", true);

    public static string Userid;

    public static string Usertoken
    {
        get => Key.GetValue("usertoken")?.ToString();
        set => Key.SetValue("usertoken", value);
    }

    public static async Task<UserInfo> GetUserInfo()
    {
        if (!Sign.IsSignin) return null;

        var parameters = new Dictionary<string, string> { { "token", Usertoken } };
        var jObject = await GetApi("https://cf-v2.uapis.cn/userinfo", parameters);
        if (jObject == null) return null;

        if ((string)jObject["state"] != "success") return null;
        var data = jObject["data"];

#if NETFRAMEWORK
        return data!.ToObject<UserInfo>();
#else
        return JsonSerializer.Deserialize<UserInfo>(data!.ToJsonString());
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