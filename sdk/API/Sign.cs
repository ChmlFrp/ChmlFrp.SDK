using static ChmlFrp.SDK.API.User;

namespace ChmlFrp.SDK.API;

public abstract class Sign
{
    public static bool IsSignin;

    public static async Task<string> Signin(string name = null, string password = null)
    {
        if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(password))
        {
            var parameters = new Dictionary<string, string>
            {
                { "username", $"{name}" },
                { "password", $"{password}" }
            };
            var jObject = await GetApi("https://cf-v2.uapis.cn/login", parameters);
            if (jObject == null || (string)jObject["state"] != "success") return jObject?["msg"]?.ToString();

            Usertoken = jObject["data"]?["usertoken"]?.ToString();
            Userid = jObject["data"]?["id"]?.ToString();
        }
        else
        {
            var parameters = new Dictionary<string, string> { { "token", Usertoken } };
            var jObject = await GetApi("https://cf-v2.uapis.cn/userinfo", parameters);

            if (jObject == null || (string)jObject["state"] != "success") return null;
            Userid = jObject["data"]?["id"]?.ToString();
        }

        IsSignin = true;
        return "登录成功";
    }

    public static void Signout()
    {
        Usertoken = "";
        IsSignin = false;
    }
}