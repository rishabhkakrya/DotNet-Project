// See https://aka.ms/new-console-template for more information

using ClientLibrary.Services;
using ClientLibrary.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((_, services) =>
    {
        services.AddHttpClient();
        services.AddMemoryCache();
        services.AddSingleton<ICacheService, CacheService>();
        services.AddSingleton<IClientService, ClientService>();
        services.AddSingleton<IExternalUserService, ExternalUserService>();
    })
    .Build();

var externalUserService = host.Services.GetRequiredService<IExternalUserService>();

Console.WriteLine("Fetching all users...");
var allUsers = await externalUserService.GetAllUsersAsync().ConfigureAwait(false);
if (allUsers != null)
    foreach (var user in allUsers)
    {
        Console.WriteLine($"User ID: {user.Id}, Name: {user.FirstName} {user.LastName}");
    }

Console.WriteLine();
Console.WriteLine("Fetching details for user with ID 2...");
var userDetails = await externalUserService.GetUserByIdAsync(2).ConfigureAwait(false);
Console.WriteLine($"User ID: {userDetails?.Id}, Name: {userDetails?.FirstName} {userDetails?.LastName}");

Console.WriteLine();

Console.WriteLine("Fetching all users from cache...");
allUsers = await externalUserService.GetAllUsersAsync().ConfigureAwait(false);
if (allUsers != null)
    foreach (var user in allUsers)
    {
        Console.WriteLine($"User ID: {user.Id}, Name: {user.FirstName} {user.LastName}");
    }

Console.WriteLine();
Console.WriteLine("Fetching details for user with ID 2 from cache...");
userDetails = await externalUserService.GetUserByIdAsync(2).ConfigureAwait(false);
Console.WriteLine($"User ID: {userDetails?.Id}, Name: {userDetails?.FirstName} {userDetails?.LastName}");
