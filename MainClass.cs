using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Win32;
#if NET48
using Newtonsoft.Json.Linq;

#else
using System.Text.Json;
using System.Text.Json.Nodes;
#endif

namespace ChmlFrp.Api;

public static class User
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
}

public abstract class Api
{
    private static async Task<object> GetApi(string url, Dictionary<string, string> parameters = null)
    {
        if (parameters != null)
            url = $"{url}?{string.Join("&", parameters.Select(pair => $"{pair.Key}={pair.Value}"))}";

        try
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();

#if NET48
            return JObject.Parse(content);
#else
        return JsonNode.Parse(content);
#endif
        }
        catch
        {
            return null;
        }
    }

    public abstract class Login
    {
        public static bool IsLogin;

        public static async Task InitializeAsync()
        {
            await Loginin();
        }

        public static async Task<string> Loginin(string name = "", string password = "")
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(password))
            {
                password = User.Password;
                name = User.Username;
            }

            User.Save(name, password);

            var parameters = new Dictionary<string, string>
            {
                { "username", $"{name}" },
                { "password", $"{password}" }
            };
            var jObject = await GetApi("https://cf-v2.uapis.cn/login", parameters);
            if (jObject == null) return "网络异常，请检查网络连接";

#if NET48
            var msg = ((JObject)jObject)["msg"]?.ToString();
            var usertoken = ((JObject)jObject)["data"]?["usertoken"]?.ToString();
#else
var msg = ((JsonNode)jObject)?["msg"]?.ToString();
var usertoken = ((JsonNode)jObject)?["data"]?["usertoken"]?.ToString();
#endif
            if (msg != "登录成功") return msg;

            User.Save(name, password, usertoken);
            IsLogin = true;
            await UserInformation.Load();
            return "登录成功";
        }

        public static void Loginout()
        {
            User.Save("", "");
            IsLogin = false;
        }
    }

    public abstract class UserInformation
    {
        public static string usergroup;
        public static string term;
        public static string qq;
        public static string email;
        public static string tunnel;
        public static string tunnelstate;
        public static string userimg;
        public static string username;
        public static string bandwidth;
        public static string abroadBandwidth;
        public static string realname;

        static UserInformation()
        {
            Load();
        }

        public static async Task Load()
        {
            if (Login.IsLogin == false) return;

            var parameters = new Dictionary<string, string> { { "token", $"{User.Usertoken}" } };
            var jObject = await GetApi("https://cf-v2.uapis.cn/userinfo", parameters);
            if (jObject == null) return;

#if NET48
            var data = ((JObject)jObject)["data"];
            usergroup = data?["usergroup"]?.ToString();
            term = data?["term"]?.ToString();
            qq = data?["qq"]?.ToString();
            email = data?["email"]?.ToString();
            tunnel = data?["tunnel"]?.ToString();
            tunnelstate = data?["tunnelstate"]?.ToString();
            userimg = data?["userimg"]?.ToString();
            username = data?["username"]?.ToString();
            bandwidth = data?["bandwidth"]?.ToString();
            abroadBandwidth = data?["abroadBandwidth"]?.ToString();
            realname = data?["realname"]?.ToString();
#else
    var data = ((JsonNode)jObject)?["data"];
    usergroup = data?["usergroup"]?.ToString();
    term = data?["term"]?.ToString();
    qq = data?["qq"]?.ToString();
    email = data?["email"]?.ToString();
    tunnel = data?["tunnel"]?.ToString();
    tunnelstate = data?["tunnelstate"]?.ToString();
    userimg = data?["userimg"]?.ToString();
    username = data?["username"]?.ToString();
    bandwidth = data?["bandwidth"]?.ToString();
    abroadBandwidth = data?["abroadBandwidth"]?.ToString();
    realname = data?["realname"]?.ToString();
#endif
        }
    }
}