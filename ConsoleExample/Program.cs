using System;
using System.Threading.Tasks;
using static System.Console;
using static ChmlFrp.SDK.TunnelActions;
using static ChmlFrp.SDK.UserActions;

await LoginAsyncFromToken();

for (;;)
{
    Clear();
    if (IsLoggedIn)
    {
        ForegroundColor = ConsoleColor.Green;
        WriteLine("Hello, World!");
        ResetColor();
        break;
    }

    ForegroundColor = ConsoleColor.Red;
    WriteLine("You are not logged in.");
    ResetColor();

    Write("用户名:");
    var username = ReadLine();
    WriteLine("密码:");
    var password = ReadLine();
    WriteLine(await LoginAsync(username, password));

    await Task.Delay(1000);
}

var tunnelList = await GetTunnelListAsync();
foreach (var tunnelInfo in tunnelList)
    StartTunnelFromId(tunnelInfo.id,
        () => WriteLine($"Tunnel {tunnelInfo.name} started successfully!"),
        () => WriteLine($"Tunnel {tunnelInfo.name} failed to start!"),
        () => WriteLine($"Tunnel {tunnelInfo.name} is running!"));

ReadKey();

foreach (var tunnelInfo in tunnelList)
    StopTunnelFromId(tunnelInfo.id,
        () => WriteLine($"Tunnel {tunnelInfo.name} stoped successfully!"));