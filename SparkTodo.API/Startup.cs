using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SparkTodo.API.Services;
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

            services.Configure<JWT.TokenOptions>(options =>
            {
                options.Audience = Configuration.GetAppSetting("TokenAudience");
                options.ValidFor = TimeSpan.FromHours(2);
                options.Issuer = Configuration.GetAppSetting("TokenIssuer");
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    // The signing key must match!
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingKey,
                    // Validate the JWT Issuer (iss) claim
                    ValidateIssuer = true,
                    ValidIssuer = Configuration.GetAppSetting("TokenIssuer"),
                    // Validate the JWT Audience (aud) claim
                    ValidateAudience = true,
                    ValidAudience = Configuration.GetAppSetting("TokenAudience"),
                    // Validate the token expiry
                    ValidateLifetime = true,
                    // If you want to allow a certain amount of clock drift, set that here:
                    ClockSkew = System.TimeSpan.Zero
                };
            });

            //Add MvcFramework
            services.AddMvcCore()
                .AddApiExplorer()
                .AddAuthorization()
                .AddDataAnnotations()
                .AddFormatterMappings()
                .AddCors()
                .AddJsonFormatters()
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.SerializerSettings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                    options.SerializerSettings.DateFormatString = "yyyy-MM-dd HH:mm:ss";
                });

            // Add api version
            // https://www.hanselman.com/blog/ASPNETCoreRESTfulWebAPIVersioningMadeEasy.aspx
            services.AddApiVersioning(options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = ApiVersion.Default;
                    options.ReportApiVersions = true;
                });
            // see https://github.com/Microsoft/aspnet-api-versioning/blob/master/samples/aspnetcore/SwaggerSample/Startup.cs for details
            //services.AddVersionedApiExplorer(options =>
            //    {
            //        // add the versioned api explorer, which also adds IApiVersionDescriptionProvider service
            //        // note: the specified format code will format the version as "'v'major[.minor][-status]"
            //        options.GroupNameFormat = "'v'VVV";

            //        // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
            //        // can also be used to control the format of the API version in route templates
            //        options.SubstituteApiVersionInUrl = true;
            //    });

            // swagger
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

                // include document file
                option.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).Assembly.GetName().Name}.xml"), true);
                // bear authentication
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

            // Add application services.
            services.AddSingleton<ITokenGenerator, TokenGenerator>();
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
        }
    }
}
