using System;
using System.Threading.Tasks;
using static System.Console;
using static ChmlFrp.SDK.TunnelActions;
using static ChmlFrp.SDK.UserActions;

if (!await AutoLoginAsync())
{
    for (;;)
    {
        Clear();

        ForegroundColor = ConsoleColor.Red;
        WriteLine("You are not logged in.");
        ResetColor();

        Write("用户名:");
        var username = ReadLine();
        Write("密码:");
        var password = ReadLine();

        if (await LoginWithCredentialsAsync(username, password, msg =>
            {
                ForegroundColor = ConsoleColor.Red;
                WriteLine(msg);
                ResetColor();
            }))
        {
            ForegroundColor = ConsoleColor.Green;
            WriteLine("Hello, World!");
            ResetColor();
            break;
        }

        await Task.Delay(1000);
    }
}
else
{
    ForegroundColor = ConsoleColor.Green;
    WriteLine("Hello, World!");
    ResetColor();
}

var tunnelList = await GetTunnelListAsync();
foreach (var tunnelInfo in tunnelList)
    StartTunnelFromId(tunnelInfo.id,
        () => WriteLine($"Tunnel {tunnelInfo.name} is running!"),
        isStart =>
            WriteLine(isStart
                ? $"Tunnel {tunnelInfo.name} started successfully!"
                : $"Tunnel {tunnelInfo.name} failed to start!")
    );

ReadKey();

foreach (var tunnelInfo in tunnelList)
    StopTunnelFromId(tunnelInfo.id,
        isStop =>
        {
            if (isStop)
                WriteLine($"Tunnel {tunnelInfo.name} stoped successfully!");
        });

ReadKey();