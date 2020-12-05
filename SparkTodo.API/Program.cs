using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus.DotNetRuntime;
using SparkTodo.Models;

namespace SparkTodo.API
{
    public class Program
    {
        /// <summary>
        /// program entry
        /// </summary>
        /// <param name="args">arguments</param>
        public static async Task Main(string[] args)
        {
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
        }
    }
}
