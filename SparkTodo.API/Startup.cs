using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Prometheus;
using SparkTodo.API.Services;
using SparkTodo.API.Swagger;
using SparkTodo.DataAccess;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            services.AddDbContextPool<SparkTodo.Models.SparkTodoDbContext>(options => options.UseInMemoryDatabase("SparkTodo"));
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

            var tokenAudience = Configuration.GetAppSetting("TokenAudience");
            var tokenIssuer = Configuration.GetAppSetting("TokenIssuer");
            services.Configure<JWT.TokenOptions>(options =>
            {
                options.Audience = tokenAudience;
                options.Issuer = tokenIssuer;
                options.ValidFor = TimeSpan.FromHours(2);
                options.SigningCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            });

            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // The signing key must match!
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        // Validate the JWT Issuer (iss) claim
                        ValidateIssuer = true,
                        ValidIssuer = tokenIssuer,
                        // Validate the JWT Audience (aud) claim
                        ValidateAudience = true,
                        ValidAudience = tokenAudience,
                        // Validate the token expiry
                        ValidateLifetime = true,
                        // If you want to allow a certain amount of clock drift, set that here:
                        ClockSkew = System.TimeSpan.FromMinutes(2)
                    };
                });

            // Add MvcFramework
            services.AddControllers();

            // Add api version
            // https://www.hanselman.com/blog/ASPNETCoreRESTfulWebAPIVersioningMadeEasy.aspx
            services.AddApiVersioning(options =>
                {
                    options.AssumeDefaultVersionWhenUnspecified = true;
                    options.DefaultApiVersion = ApiVersion.Default;
                    options.ReportApiVersions = true;
                });

            // swagger
            // https://stackoverflow.com/questions/58197244/swaggerui-with-netcore-3-0-bearer-token-authorization
            services.AddSwaggerGen(option =>
            {
                option.SwaggerDoc("spark todo", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "SparkTodo API",
                    Description = "API for SparkTodo",
                    Contact = new OpenApiContact() { Name = "WeihanLi", Email = "weihanli@outlook.com" }
                });

                option.SwaggerDoc("v1", new OpenApiInfo { Version = "v1", Title = "API V1" });
                option.SwaggerDoc("v2", new OpenApiInfo { Version = "v2", Title = "API V2" });

                option.DocInclusionPredicate((docName, apiDesc) =>
                {
                    var versions = apiDesc.CustomAttributes()
                        .OfType<ApiVersionAttribute>()
                        .SelectMany(attr => attr.Versions);

                    return versions.Any(v => $"v{v}" == docName);
                });

                option.OperationFilter<RemoveVersionParameterOperationFilter>();
                option.DocumentFilter<SetVersionInPathDocumentFilter>();

                // include document file
                option.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{typeof(Startup).Assembly.GetName().Name}.xml"), true);

                option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Description = "Please enter into field the word 'Bearer' followed by a space and the JWT value",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                });
                option.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    { new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference()
                        {
                            Id = "Bearer",
                            Type = ReferenceType.SecurityScheme
                        }
                    }, Array.Empty<string>() }
                });
            });

            // Add application services.
            services.AddSingleton<ITokenGenerator, TokenGenerator>();
            //Repository
            services.RegisterAssemblyTypesAsImplementedInterfaces(t => t.Name.EndsWith("Repository"),
                ServiceLifetime.Scoped, typeof(IUserAccountRepository).Assembly);
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">app</param>
        public void Configure(IApplicationBuilder app)
        {
            // disable claimType transform, see details here https://stackoverflow.com/questions/39141310/jwttoken-claim-name-jwttokentypes-subject-resolved-to-claimtypes-nameidentifie
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

            // emit dotnet rumtime version to response header
            app.Use(async (context, next) =>
            {
                context.Response.Headers["DotNetVersion"] = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
                await next();
            });

            //Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();
            //Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endpoint
            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint("/swagger/v2/swagger.json", "V2 Docs");
                option.SwaggerEndpoint("/swagger/v1/swagger.json", "V1 Docs");

                option.RoutePrefix = string.Empty;
                option.DocumentTitle = "SparkTodo API";
            });

            app.UseRouting();
            app.UseCors(builder=>
            {
                builder.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_=>true);
            });

            app.UseHttpMetrics();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapMetrics();
                endpoints.MapControllers();
            });
        }
    }
}
