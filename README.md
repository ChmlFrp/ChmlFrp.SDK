# ChmlFrp.SDK

## 介绍

一个方便.NET开发者开发第三方ChmlFrp客户端的Nuget包。

## 安装

```bash
dotnet add package ChmlFrp.SDK
```

## 使用
> 注意：如果未登录使用，其他方法返回为null。

### 登录

```csharp
using ChmlFrp.SDK.API;

// 之前输入的用户名和密码
YourTextBox.Text = User.Username; 
// 登录状态的提示。
var msg = await Sign.Signin(【你的用户名】,【你的密码】);
// 登录状态。
if (Sign.IsSignin) Console.WriteLine("登录成功");
else Console.WriteLine("登录失败");
```

### 登出

```csharp
using ChmlFrp.SDK.API;

Sign.Signout();
```

### 用户信息

```csharp
uusing ChmlFrp.SDK;

// 以email为例
var userInfo = await User.GetUserInfo();
if (userInfo == null) return;
Console.WriteLine(userInofo.email);
```

### 获得隧道名列表
```csharp
using ChmlFrp.SDK.API;

var tunnelnames = await Tunnel.GetTunnelNames();
if (tunnelnames == null) return;
foreach(var tunnelname in tunnelnames) Console.WriteLine(tunnelname);
```

### 获得隧道信息
```csharp
using ChmlFrp.SDK.API;

// 以type为例
var tunnelData = await Tunnel.GetTunnelData(【你的隧道名】);
if (tunnelData == null) return;
Console.WriteLine(tunnelData.type);
```

### 获得隧道配置文件

```csharp
using ChmlFrp.SDK.API;

// 需要登录后才能获取隧道配置文件
var iniData = await Tunnel.GetTunnelIniData(【你的隧道名】);
if (iniData == null) return;
File.WriteAllText(【隧道配置文件路径】, iniData);
```