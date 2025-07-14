# 使用

## 准备工作

在开始使用 SDK 之前，请确保：

1. 已通过 NuGet 安装 `ChmlFrp.SDK` 包
2. 了解基本的异步编程概念

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

以下主要介绍了如何在 .NET 项目中集成和调用 ChmlFrp.SDK 的常用功能，包括登录、登出、获取用户信息、隧道管理、文件目录初始化等。每个功能都配有简明的
C# 示例代码，便于开发者快速上手。所有接口均为异步方法，适合现代 .NET 应用开发。

<br/>

## 登录

```csharp
using ChmlFrp.SDK.API;

// 用户名密码登录
string result = await Sign.Signin("your_username", "your_password");
if (Sign.IsSignin) {
    // 登录成功
}
```

## 登出

```csharp
using ChmlFrp.SDK.API;

Sign.Signout();

if (!Sign.IsSignin) {
    // 登出成功
}
```

## 初始化日志与环境

```csharp
using ChmlFrp.SDK;

Paths.Init("MyApp"); // 日志文件名可自定义
```

## 获取用户信息

```csharp
using ChmlFrp.SDK.API;

var userInfo = await User.GetUserInfo();
if (userInfo != null) {
    Console.WriteLine(userInfo.username);
}
```

## 获取所有隧道

```csharp
using ChmlFrp.SDK.API;

var tunnels = await Tunnel.GetTunnelsData();
foreach (var t in tunnels) {
    Console.WriteLine(t.name);
}
```

## 获取单个隧道信息

```csharp
using ChmlFrp.SDK.API;

var tunnel = await Tunnel.GetTunnelData("tunnel_name");
if (tunnel != null) {
    Console.WriteLine(tunnel.ip);
}
```

## 获取隧道配置（ini）

```csharp
using ChmlFrp.SDK.API;

var ini = await Tunnel.GetTunnelIniData("tunnel_name");
Console.WriteLine(ini);
```

## 创建隧道

```csharp
using ChmlFrp.SDK.API;

var error = Tunnel.CreateTunnel("节点名", "tcp", "本地端口", "远程端口");

if (error == "创建成功") {
    Console.WriteLine("隧道创建成功");
} 
```

## 删除隧道

```csharp
using ChmlFrp.SDK.API;

Tunnel.DeleteTunnel("tunnel_name");
```

## 启动/停止/检测隧道进程

```csharp
using ChmlFrp.SDK.Services;

// 启动隧道
Tunnel.StartTunnel("tunnel_name",
    onStartTrue: () => Console.WriteLine("启动成功"),
    onStartFalse: () => Console.WriteLine("启动失败"),
    onIniUnKnown: () => Console.WriteLine("配置未知"),
    onFrpcNotExists: () => Console.WriteLine("frpc.exe 不存在"),
    onTunnelRunning: () => Console.WriteLine("隧道已在运行")
);

// 停止隧道
await Tunnel.StopTunnel("tunnel_name",
    onStopTrue: () => Console.WriteLine("已停止"),
    onStopFalse: () => Console.WriteLine("未运行")
);

// 检查隧道是否运行
bool isRunning = await Tunnel.IsTunnelRunning("tunnel_name");
Console.WriteLine(isRunning ? "运行中" : "未运行");
```

## 节点信息

```csharp
using ChmlFrp.SDK.API;

var nodes = await Node.GetNodeData();
foreach (var node in nodes) {
    Console.WriteLine(node.name);
}
```

> 注：以上示例代码不适用于1.3.0版本。