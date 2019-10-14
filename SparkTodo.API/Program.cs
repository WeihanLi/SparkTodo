using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SparkTodo.Models;

namespace SparkTodo.API
{
    public class Program
    {
        /// <summary>
        /// program entry
        /// </summary>
        /// <param name="args">arguments</param>
        public static void Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webHostBuilder =>
                {
                    webHostBuilder.UseStartup<Startup>();
                })
                .Build();

            using (var serviceScope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<SparkTodo.Models.SparkTodoDbContext>();
                dbContext.Database.EnsureCreated();
                //init Database,you can add your init data here
                var userManager = serviceScope.ServiceProvider.GetService<UserManager<UserAccount>>();

                var email = "weihanli@outlook.com";
                if (userManager.FindByEmailAsync(email).GetAwaiter().GetResult() == null)
                {
                    userManager.CreateAsync(new UserAccount
                    {
                        UserName = "weihanli@outlook.com",
                        Email = "weihanli@outlook.com"
                    }, "Test1234").Wait();
                }
            }
            host.Run();
        }
    }
}
