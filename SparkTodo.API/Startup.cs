using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace SparkTodo.API
{
    /// <summary>
    /// StartUp
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// StartUp .ctor
        /// </summary>
        /// <param name="env">HostingEnvironment</param>
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

        /// <summary>
        /// Configuration
        /// </summary>
        public IConfigurationRoot Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<SparkTodo.Models.SparkTodoEntity>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));
            //
            services.AddIdentity<SparkTodo.Models.UserAccount, Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole>()
                .AddEntityFrameworkStores<SparkTodo.Models.SparkTodoEntity>()
                .AddDefaultTokenProviders();
            // Add session
            services.AddSession(options => options.IdleTimeout = System.TimeSpan.FromMinutes(20));
            //Add MvcFramewok
            services.AddMvc();
            services.AddSwaggerGen(option => {
                option.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Version = "v1",
                    Title = "SparkTodo API",
                    Description = "API for SparkTodo",
                    TermsOfService = "None",
                    Contact = new Swashbuckle.AspNetCore.Swagger.Contact { Name = "WeihanLi" , Email="weihanli@outlook.com"}
                });
                //Set the comments path for the swagger json and ui.
                var basePath = Microsoft.Extensions.PlatformAbstractions.PlatformServices.Default.Application.ApplicationBasePath;
                var xmlPath = System.IO.Path.Combine(basePath, "SparkTodo.API.xml");
                option.IncludeXmlComments(xmlPath);
            });

            // WebApiSettings services.Configure<WebApiSettings>(settings => settings.HostName =
            // Configuration["HostName"]);
            services.Configure<Models.WebApiSettings>(settings => settings.SecretKey = Configuration["SecretKey"]);
            // Add application services.
            //Repository
            services.AddScoped<SparkTodo.DataAccess.ICategoryRepository, SparkTodo.DataAccess.Repository.CategoryRepository>();
            services.AddScoped<SparkTodo.DataAccess.ITodoItemRepository, SparkTodo.DataAccess.Repository.TodoItemRepository>();
            services.AddScoped<SparkTodo.DataAccess.IUserAccountRepository, SparkTodo.DataAccess.Repository.UserAccountRepository>();
            services.AddScoped<SparkTodo.Models.SparkTodoEntity>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline. 
        /// </summary>
        /// <param name="app">app</param>
        /// <param name="env">environment</param>
        /// <param name="loggerFactory">loggerFactory</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //    app.UseDatabaseErrorPage();
            //    app.UseBrowserLink();
            //}
            //else
            //{
            //    app.UseExceptionHandler("/Home/Error");
            //}

            app.UseDeveloperExceptionPage();
            app.UseDatabaseErrorPage();
            app.UseBrowserLink();

            // Add ASP.NET Core Identity
            app.UseIdentity().UseCookieAuthentication(
                new CookieAuthenticationOptions()
                {
                    AccessDeniedPath = new PathString("/api/v1/Account/SignIn"),
                    LoginPath = new PathString("/api/v1/Account/SignIn")
                });
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
                ClockSkew = System.TimeSpan.Zero
            };
            app.UseJwtBearerAuthentication(new JwtBearerOptions
            {
                AutomaticAuthenticate = true,
                AutomaticChallenge = true,
                TokenValidationParameters = tokenValidationParameters
            });

            app.UseStaticFiles();
            app.UseSession();            

            app.UseMvcWithDefaultRoute();
            //Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            //Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint
            app.UseSwaggerUI(option => {
                option.SwaggerEndpoint("/swagger/v1/swagger.json", "SparkTodo API v1");
            });

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