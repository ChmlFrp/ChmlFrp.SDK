# ChmlFrp.SDK

## 介绍

该项目 ChmlFrp.SDK 是一个为 .NET 开发者提供的第三方 ChmlFrp 客户端开发 Nuget
包。它封装了用户登录、登出、用户信息获取、隧道管理、配置文件操作、日志与目录初始化等常用功能，简化了集成 ChmlFrp 的流程

## 使用

> 注意：如果未登录使用用户相关方法，会返回为null。

### 登录

以下主要介绍了如何在 .NET 项目中集成和调用 ChmlFrp.SDK 的常用功能，包括登录、登出、获取用户信息、隧道管理、文件目录初始化等。每个功能都配有简明的
C# 示例代码，便于开发者快速上手。所有接口均为异步方法，适合现代 .NET 应用开发。

```csharp
using ChmlFrp.SDK.API;

// 之前输入的用户名和密码
YourTextBox.Text = User.Username; 
// 登录状态的提示
var msg = await Sign.Signin(【你的用户名】,【你的密码】);
Console.WriteLine(msg);
// 登录状态
Console.WriteLine(Sign.IsSignin.ToString);
```

### 登出

```csharp
using ChmlFrp.SDK.API;

Sign.Signout();
```

### 用户信息

```csharp
using ChmlFrp.SDK;

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

var iniData = await Tunnel.GetTunnelIniData(【你的隧道名】);
if (iniData == null) return;
File.WriteAllText(【隧道配置文件路径】, iniData);
```

### 初始化文件目录

```csharp
using System.IO;
using ChmlFrp.SDK;

// 可以自定义创建文件夹
CreateDictionaryList.Add(Path.Combine(Paths.DataPath, 【文件夹名】));

// 在Init时创建文件夹，日志文件和下载FRPC文件
Paths.Init(【你的启动器缩写】);
// 例如：CAT2 / CAT / CUL
// 它会生成你输入的名称为名字的log文件

// 如果你要使用log请使用
Paths.WritingLog(【日志】);

// 检测FRPC文件是否存在
Console.WriteLine(Paths.IsFrpcExists.ToString());
```

### 启动和关闭隧道

关于启动和关闭隧道，相关使用请查看[CAT2示例代码](https://github.com/ChmlFrp/CAT2/blob/main/cat2/Views/Pages/TunnelPage.xaml.cs)。