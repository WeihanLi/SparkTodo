using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Prometheus.DotNetRuntime;
using SparkTodo.API;
using SparkTodo.Models;

DotNetRuntimeStatsBuilder.Customize()
    .WithContentionStats()
    .WithGcStats()
    .WithThreadPoolStats()
    .StartCollecting();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureWebHostDefaults(webHostBuilder =>
    {
        webHostBuilder.UseStartup<Startup>();
    })
    .ConfigureLogging(loggingBuilder =>
    {
        loggingBuilder.AddJsonConsole();
    })
    .Build();

using (var serviceScope = host.Services.CreateScope())
{
    var dbContext = serviceScope.ServiceProvider.GetRequiredService<SparkTodoDbContext>();
    await dbContext.Database.EnsureCreatedAsync();

    //init Database,you can add your init data here
    var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<UserAccount>>();
    var email = "weihanli@outlook.com";
    if (await userManager.FindByEmailAsync(email) == null)
    {
        await userManager.CreateAsync(new UserAccount
        {
            UserName = email,
            Email = email
        }, "Test1234");
    }
}

await host.RunAsync();
