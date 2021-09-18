using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SparkTodo.DataAccess;
using SparkTodo.Models;
using SparkTodo.WebExtension;
using SparkTodo.WebExtension.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Services.AddBrowserExtensionServices(options =>
    {
        options.ProjectNamespace = typeof(Program).Namespace;
    });
builder.Services.AddDbContextPool<SparkTodoDbContext>(options =>
    options.UseInMemoryDatabase("SparkTodo")
);
builder.Services.AddSingleton<TodoScheduler>();
//Repository
builder.Services.RegisterAssemblyTypesAsImplementedInterfaces(t => t.Name.EndsWith("Repository"),
    ServiceLifetime.Scoped, typeof(IUserAccountRepository).Assembly);
var host = builder.Build();
await host.Services.GetRequiredService<TodoScheduler>()
    .Start();
await host.RunAsync();
