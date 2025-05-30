# ChmlFrp.SDK

## 介绍

一个方便.NET开发者开发第三方ChmlFrp客户端的Nuget包。

## 使用

> 注意：如果未登录使用用户相关方法，会返回为null。

### 登录

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

var iniData = await Tunnel.GetTunnelIniData(【你的隧道名】);
if (iniData == null) return;
File.WriteAllText(【隧道配置文件路径】, iniData);
```

### 初始化文件目录

```csharp
using System.IO;
using ChmlFrp.SDK;

// 可以自定义创建文件夹
Paths.CreateDictionaryList = 
[
    $"{Paths.DataPath}",
    Path.Combine(Paths.DataPath, 【文件夹名】)
]

// 在Init时创建文件夹，日志文件和下载FRPC文件
Paths.Init(【你的启动器缩写】);
// 例如：CAT2 / CAT / CUL
// 它会生成你输入的名称为名字的log文件

// 如果你要使用log请使用
Paths.WritingLog(【日志】);

// 检测FRPC文件是否存在
Console.WriteLine(Paths.IsFrpcExists.ToString());
```