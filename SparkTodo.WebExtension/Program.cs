// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.Services.AddBrowserExtensionServices();
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
