# ChmlFrp.Api

一个用于获得ChmlFrp API的.NET Nuget包。

## 安装

```bash
dotnet add package ChmlFrp.Api
```

如果是.Net9.0直接可以安装
如果是.Net48需要安装Newtonsoft.Json。

## 使用

### 登录

```csharp
using ChmlFrp.Api;

// 如何想让TextBox或PasswordBox获取之前输入的错误的用户名和密码
YourTextBox.Text = User.Username; // 密码同理

// 输入用户名密码登录，返回的是登录状态的提示。
string msg = await Api.Sign.Signin(【你的用户名】,【你的密码】);

// 这才是登录状态。
if (Api.Sign.IsSignin) Console.WriteLine("登录成功");
else Console.WriteLine("登录失败");
```

### 登出

```csharp
using ChmlFrp.Api;

Api.Sign.Signout();
// 登出后登录状态会变成false
// 注：登出后用户信息没有变成null。
```

### 用户信息

```csharp
using ChmlFrp.Api;

//这是完整信息显示。
string UserInfo = Api.UserInfo.GetUserInfo;
if (UserInfo == null) return;
Console.WriteLine(UserInfo);
//这是单个的以email为例
string email = Api.UserInfo.email;
if (email == null) return;
Console.WriteLine(email);
// 因为用户信息是登录后才能获取的，所以需要先登录，不然会返回null。
```

### 获得隧道配置文件

```csharp
using ChmlFrp.Api;

// 需要登录后才能获取隧道配置文件
string iniData =  Api.Tunnel.GetTunnelIniData(【你的隧道名】);
if (iniData == null) return;
File.WriteAllText(【隧道配置文件路径】, iniData);
```