#if NET48
using Newtonsoft.Json.Linq;
#else
using System.Text.Json.Nodes;
#endif
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace ChmlFrp.Api;

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

    public static class Sign
    {
        public static bool IsSignin;

        static Sign()
        {
#pragma warning disable CS4014
            Signin();
#pragma warning restore CS4014
        }

        public static async Task<string> Signin(string name = "", string password = "")
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
            IsSignin = true;
            await UserInfo.Load();
            return "登录成功";
        }

        public static void Signout()
        {
            User.Save("", "");
            IsSignin = false;
        }
    }

    public abstract class Tunnel
    {
        public static async Task<string> GetTunnelIniData(string name)
        {
            if (!Sign.IsSignin) return null;

            var parameters = new Dictionary<string, string> { { "token", $"{User.Usertoken}" } };
            var jObject = await GetApi("https://cf-v2.uapis.cn/tunnel", parameters);

            if (jObject == null) return null;

#if NET48
            var data = ((JObject)jObject)["data"];
            var msg = ((JObject)jObject)["msg"];

            if (data == null) return null;
            if (msg?.ToString() != "获取隧道数据成功") return null;
            var node = (from tunnel in data
                where tunnel["name"]?.ToString() == name
                select tunnel["node"]?.ToString()).FirstOrDefault();
#else
            var data = ((JsonNode)jObject)["data"];
            var msg = ((JsonNode)jObject)["msg"];

            if (data == null) return null;
            if (msg?.ToString() != "获取隧道数据成功") return null;

            var node = (from tunnel in data.AsArray()
                where tunnel!["name"]?.ToString() == name
                select tunnel["node"]?.ToString()).FirstOrDefault();
#endif

            if (node == null) return null;

            parameters = new Dictionary<string, string>
            {
                { "token", $"{User.Usertoken}" },
                { "node", $"{node}" },
                { "tunnel_names", $"{name}" }
            };
            jObject = await GetApi("https://cf-v2.uapis.cn/tunnel_config", parameters);

#if NET48
            data = ((JObject)jObject)["data"];
            msg = ((JObject)jObject)["msg"];
#else
            data = ((JsonNode)jObject)?["data"];
            msg = ((JsonNode)jObject)?["msg"];
#endif
            if (msg?.ToString() != "配置文件获取成功") return null;

            return (string)data;
        }
    }

    public abstract class UserInfo
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

        public static async Task Load()
        {
            if (Sign.IsSignin == false) return;

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
            var data = ((JsonNode)jObject)["data"];
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


        public static string GetUserInfo()
        {
            return $"用户名：{username}\n" +
                   $"用户组：{usergroup}\n" +
                   $"到期时间：{term}\n" +
                   $"QQ：{qq}\n" +
                   $"邮箱：{email}\n" +
                   $"隧道数：{tunnel}\n" +
                   $"隧道状态：{tunnelstate}\n" +
                   $"头像：{userimg}\n" +
                   $"带宽：{bandwidth}\n" +
                   $"境外带宽：{abroadBandwidth}\n" +
                   $"实名信息：{realname}";
        }
    }
}