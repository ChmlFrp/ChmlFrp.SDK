using System.Text.Json;

namespace ChmlFrp.SDK.API;

public abstract class User
{
    public static UserInfoClass UserInfo;
    public static string Userid => UserInfo.id.ToString();
    public static string Usertoken => UserInfo.usertoken;

    public static async void GetUserInfo()
    {
        if (!Sign.IsSignin) return;

        var parameters = new Dictionary<string, string> { { "token", Usertoken } };
        var jObject = await GetApi("https://cf-v2.uapis.cn/userinfo", parameters);
        if (jObject == null) return;

        UserInfo = (string)jObject["state"] != "success"
            ? null
            : JsonSerializer.Deserialize<UserInfoClass>(jObject["data"]!.ToJsonString());
    }

    public class UserInfoClass
    {
        public string username { get; set; }
        public string usergroup { get; set; }
        public string userimg { get; set; }
        public string usertoken { get; set; }
        public long id { get; set; }
        public string term { get; set; }
        public string qq { get; set; }
        public string email { get; set; }
        public int bandwidth { get; set; }
        public int tunnel { get; set; }
        public int tunnelCount { get; set; }
        public string realname { get; set; }
        public string regtime { get; set; }
        public int integral { get; set; }
        public long total_upload { get; set; }
        public long total_download { get; set; }
    }
}