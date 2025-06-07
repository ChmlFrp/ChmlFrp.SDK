# 使用

## 准备工作

在开始使用 SDK 之前，请确保：

1. 已通过 NuGet 安装 `ChmlFrp.SDK` 包
2.了解基本的异步编程概念

## 核心类型概览

SDK 主要包含以下核心类型：

- `Sign` - 处理登录、登出相关操作
- `User` - 用户信息管理
- `Tunnel` - 隧道配置和操作
- `Paths` - 文件系统和日志管理

## 重要说明

- 所有网络请求方法都是异步的，需要使用 `await` 关键字
- 未登录状态下调用用户相关方法将返回 `null`
- 请妥善保管用户凭据，建议使用安全的配置管理方式
- 确保应用具有足够的文件系统权限以进行日志写入和配置文件操作

以下主要介绍了如何在 .NET 项目中集成和调用 ChmlFrp.SDK 的常用功能，包括登录、登出、获取用户信息、隧道管理、文件目录初始化等。每个功能都配有简明的 C# 示例代码，便于开发者快速上手。所有接口均为异步方法，适合现代 .NET 应用开发。

<br/>

## 登录

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

## 登出

```csharp
using ChmlFrp.SDK.API;

Sign.Signout();
```

## 用户信息

```csharp
using ChmlFrp.SDK;

// 以email为例
var userInfo = await User.GetUserInfo();
if (userInfo == null) return;
Console.WriteLine(userInofo.email);
```

## 获得隧道名列表

```csharp
using ChmlFrp.SDK.API;

var tunnelnames = await Tunnel.GetTunnelNames();
if (tunnelnames == null) return;
foreach(var tunnelname in tunnelnames) Console.WriteLine(tunnelname);
```

## 获得隧道信息

```csharp
using ChmlFrp.SDK.API;

// 以type为例
var tunnelData = await Tunnel.GetTunnelData(【你的隧道名】);
if (tunnelData == null) return;
Console.WriteLine(tunnelData.type);
```

## 获得隧道配置文件

```csharp
using ChmlFrp.SDK.API;

var iniData = await Tunnel.GetTunnelIniData(【你的隧道名】);
if (iniData == null) return;
File.WriteAllText(【隧道配置文件路径】, iniData);
```

## 初始化文件目录

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

## 启动和关闭隧道

关于启动和关闭隧道，相关使用请查看[CAT2示例代码](https://github.com/ChmlFrp/CAT2/blob/main/cat2/Views/Pages/TunnelPage.xaml.cs)。