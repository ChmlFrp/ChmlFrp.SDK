namespace ChmlFrp.SDK.API;

public abstract class Sign
{
    public static bool IsSignin;

    public static async Task<string> Signin(string name = null, string password = null)
    {
        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
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

        var msg = jObject["msg"]?.ToString();
        Paths.WritingLog($"Login results: {msg}");
        if ((string)jObject["state"] != "success") return msg;

        var usertoken = jObject["data"]?["usertoken"]?.ToString();
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