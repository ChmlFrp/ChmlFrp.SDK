#if NETFRAMEWORK
using Newtonsoft.Json.Linq;

#else
using System.Text.Json.Nodes;
#endif

namespace ChmlFrp.SDK.API;

public abstract class Sign
{
    public static bool IsSignin;

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
        var jObject = await Constant.GetApi("https://cf-v2.uapis.cn/login", parameters);
        if (jObject == null) return "网络异常，请检查网络连接";

#if NETFRAMEWORK
        var msg = ((JObject)jObject)["msg"]?.ToString();
        var usertoken = ((JObject)jObject)["data"]?["usertoken"]?.ToString();
#else
        var msg = ((JsonNode)jObject)?["msg"]?.ToString();
        var usertoken = ((JsonNode)jObject)?["data"]?["usertoken"]?.ToString();
#endif

        Paths.WritingLog($"Login results: {msg}");
        if (msg != "登录成功") return msg;

        User.Save(name, password, usertoken);
        IsSignin = true;
        return msg;
    }

    public static void Signout()
    {
        User.Clear();
        IsSignin = false;
    }
}