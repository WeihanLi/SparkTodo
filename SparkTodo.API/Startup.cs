using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace SparkTodo.API
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            //if (env.IsDevelopment())
            //{
            //    // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
            //    builder.AddUserSecrets();
            //}

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //Repository
            services.AddScoped<SparkTodo.DataAccess.ICategoryRepository,SparkTodo.DataAccess.Repository.CategoryRepository>();
                    services.AddScoped<SparkTodo.DataAccess.ITodoItemRepository,SparkTodo.DataAccess.Repository.TodoItemRepository>();
                    services.AddScoped<SparkTodo.DataAccess.IUserAccountRepository,SparkTodo.DataAccess.Repository.UserAccountRepository>();
            services.AddScoped<SparkTodo.Models.SparkTodoEntity>();
            // Add framework services.
            services.AddDbContext<SparkTodo.Models.SparkTodoEntity>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            // Add session
            services.AddSession(options => options.IdleTimeout = System.TimeSpan.FromMinutes(20));
            //Add MvcFramewok
            services.AddMvc();
            // WebApiSettings
            // services.Configure<WebApiSettings>(settings => settings.HostName = Configuration["HostName"]);
            services.Configure<Models.WebApiSettings>(settings => settings.SecretKey = Configuration["SecretKey"]);
            // Add application services.
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            // if (env.IsDevelopment())
            // {
            //     app.UseDeveloperExceptionPage();
            //     app.UseDatabaseErrorPage();
            //     app.UseBrowserLink();
            // }
            // else
            // {
            //     app.UseExceptionHandler("/Home/Error");
            // }
            
            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            app.UseBrowserLink();

            // Add JWT¡¡Protection
            var secretKey = Configuration["SecretKey"];
            var signingKey = new SymmetricSecurityKey(System.Text.Encoding.ASCII.GetBytes(secretKey));
            var tokenValidationParameters = new TokenValidationParameters
            {
                // The signing key must match! 
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = signingKey,
                // Validate the JWT Issuer (iss) claim 
                ValidateIssuer = true,
                ValidIssuer = "SparkTodo",
                // Validate the JWT Audience (aud) claim 
                ValidateAudience = true,
                ValidAudience = "SparkTodoAudience",
                // Validate the token expiry 
                ValidateLifetime = true,
                // If you want to allow a certain amount of clock drift, set that here: 
                ClockSkew = System.TimeSpan.FromMinutes(20)
            };
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters
            });

            app.UseStaticFiles();
            app.UseSession();
            app.UseMvc();
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<SparkTodo.Models.SparkTodoEntity>();
                bool HasCreated = dbContext.Database.EnsureCreated();
                //init Database,you can add your init data here
            }
        }
    }
}
