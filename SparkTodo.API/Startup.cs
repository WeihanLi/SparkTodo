using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Swashbuckle.AspNetCore.Swagger;
using WeihanLi.Common;

namespace SparkTodo.API
{
    /// <summary>
    /// StartUp
    /// </summary>
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration.ReplacePlaceholders();
        }

        /// <summary>
        /// Configuration
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">ServiceCollection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<SparkTodo.Models.SparkTodoDbContext>(options => options.UseInMemoryDatabase("SparkTodo"));
            //
            services.AddIdentity<SparkTodo.Models.UserAccount, SparkTodo.Models.UserRole>(options =>
                {
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredUniqueChars = 0;
                    options.User.RequireUniqueEmail = true;
                })
                .AddEntityFrameworkStores<SparkTodo.Models.SparkTodoDbContext>()
                .AddDefaultTokenProviders();

            // Add JWT token validation
            var secretKey = Configuration.GetAppSetting("SecretKey");
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

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Audience = "SparkTodoAudience";
                options.TokenValidationParameters = tokenValidationParameters;
            });

            //Add MvcFramewok
            services.AddMvc()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                });

            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("sparktodo", new Swashbuckle.AspNetCore.Swagger.Info
                {
                    Version = "v1",
                    Title = "SparkTodo API",
                    Description = "API for SparkTodo",
                    TermsOfService = "None",
                    Contact = new Contact { Name = "WeihanLi", Email = "weihanli@outlook.com" }
                });

                option.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
                    $"{typeof(Startup).Assembly.GetName().Name}.xml"), true);

                option.AddSecurityDefinition("Bearer", new ApiKeyScheme
                {
                    Name = "Authorization",
                    In = "header",
                    Description = "Bearer token"
                });
                option.AddSecurityRequirement(new Dictionary<string, IEnumerable<string>>
                {
                    { "Bearer", Enumerable.Empty<string>() }
                });
            });

            // WebApiSettings services.Configure<WebApiSettings>(settings => settings.HostName =
            // Configuration["HostName"]);
            services.Configure<Models.WebApiSettings>(settings => settings.SecretKey = Configuration["SecretKey"]);

            // Add application services.

            //Repository
            services.AddScoped<DataAccess.ICategoryRepository, DataAccess.CategoryRepository>();
            services.AddScoped<DataAccess.ITodoItemRepository, DataAccess.TodoItemRepository>();
            services.AddScoped<DataAccess.IUserAccountRepository, DataAccess.UserAccountRepository>();

            // Set to DependencyResolver
            DependencyResolver.SetDependencyResolver(services);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">app</param>
        /// <param name="loggerFactory">loggerFactory</param>
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddLog4Net();

            //Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            //Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint
            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint("/swagger/sparktodo/swagger.json", "SparkTodo API");
                option.RoutePrefix = string.Empty;
                option.DocumentTitle = "SparkTodo API";
            });

            app.UseMvc();
            System.Console.OutputEncoding = System.Text.Encoding.UTF8;
        }
    }
}
