using System.Diagnostics;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using RestartApplicationPool.Share.Enums;

var builder = new ConfigurationBuilder()
              .SetBasePath(Directory.GetCurrentDirectory())
              .AddJsonFile("appsettings.json", optional: false);

var config = builder.Build();

var hubUrl = config.GetSection("Hub").Value;

if (string.IsNullOrEmpty(hubUrl))
{
    Console.WriteLine("Hub Url can't be null!!");
    return;
}

var processStartInfo = new ProcessStartInfo("cmd.exe");
processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
processStartInfo.Verb = "runas";
processStartInfo.UseShellExecute = false;
processStartInfo.RedirectStandardOutput = true;
processStartInfo.RedirectStandardError = true;

var connection = new HubConnectionBuilder()
                .WithUrl(hubUrl)
                .WithAutomaticReconnect()
                .Build();

connection.Closed += async (error) =>
{
    await Task.Delay(new Random().Next(0, 5) * 1000);
    await connection.StartAsync();
};

connection.On<int>("ReceiveMessage", args =>
{
    Console.WriteLine(args.ToString());
    string pool = string.Empty;
    switch (args)
    {
        case (int)ActionType.RestartApplicationPoolCMS:
            pool = "CMS";
            break;
        default:
            pool = "Livesite";
            break;
    }

    try
    {
        processStartInfo.Arguments = $"\"%windir%\\system32\\inetsrv\\appcmd\" stop APPPOOL \"{pool}\"";
        Process.Start(processStartInfo);

        Console.WriteLine($"{pool} stopped");

        Thread.Sleep(3000);
        processStartInfo.Arguments = $"\"%windir%\\system32\\inetsrv\\appcmd\" start APPPOOL \"{pool}\"";
        Process.Start(processStartInfo);

        Console.WriteLine($"{pool} started");

        connection.SendAsync("ReturnMessage", $"{pool} restart success!");
    }
    catch (Exception ex)
    {
        connection.SendAsync("ReturnMessage", $"{pool} restart failure!");
    }
});

await connection.StartAsync();
Console.WriteLine("Connection started");

Console.ReadLine();