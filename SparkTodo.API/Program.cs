// Copyright (c) Weihan Li. All rights reserved.
// Licensed under the MIT license.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Octokit.Webhooks;
using Octokit.Webhooks.AspNetCore;
using Prometheus;
using SparkTodo.API.Services;
using SparkTodo.API.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using WeihanLi.EntityFramework.Interceptors;
using WeihanLi.Web.Authorization.Jwt;
using WeihanLi.Web.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.AddJsonConsole(options =>
{
    options.JsonWriterOptions = new JsonWriterOptions
    {
        // https://docs.microsoft.com/en-us/dotnet/standard/serialization/system-text-json-character-encoding?WT.mc_id=DT-MVP-5004222
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
});

// Add framework services.
builder.Services.AddDbContext<SparkTodoDbContext>(options => 
{
    options.UseSqlite("Data Source=SparkTodo.db");
    options.AddInterceptors(new SoftDeleteInterceptor());
});
//
builder.Services.AddIdentity<UserAccount, UserRole>(options =>
{
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredUniqueChars = 0;
    options.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<SparkTodoDbContext>()
    .AddDefaultTokenProviders();

// Add JWT token validation
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer();

var secretKey = builder.Configuration.GetAppSetting("SecretKey");
ArgumentNullException.ThrowIfNull(secretKey);
var tokenAudience = builder.Configuration.GetAppSetting("TokenAudience");
var tokenIssuer = builder.Configuration.GetAppSetting("TokenIssuer");
builder.Services.AddJwtTokenServiceWithJwtBearerAuth(options =>
{
    options.Audience = tokenAudience;
    options.Issuer = tokenIssuer;
    options.ValidFor = TimeSpan.FromHours(2);
    options.SecretKey = secretKey;
    options.EnableRefreshToken = true;
});

// Add MvcFramework
builder.Services.AddControllers();
// Add api version
// https://www.hanselman.com/blog/ASPNETCoreRESTfulWebAPIVersioningMadeEasy.aspx
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = ApiVersion.Default;
    options.ReportApiVersions = true;
});
// swagger
// https://stackoverflow.com/questions/58197244/swaggerui-with-netcore-3-0-bearer-token-authorization
builder.Services.AddSwaggerGen(option =>
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
    option.IncludeXmlComments(Assembly.GetExecutingAssembly(), true);

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
builder.Services.AddHealthChecks();
// Add application services.
builder.Services.AddSingleton<IKubernetesService, KubernetesService>();
//Repository
builder.Services.RegisterAssemblyTypesAsImplementedInterfaces(t => t.Name.EndsWith("Repository"),
    ServiceLifetime.Scoped, typeof(IUserAccountRepository).Assembly);

builder.Services.AddSingleton<WebhookEventProcessor, MyWebhookEventProcessor>();

var app = builder.Build();

// Disable claimType transform, see details here https://stackoverflow.com/questions/39141310/jwttoken-claim-name-jwttokentypes-subject-resolved-to-claimtypes-nameidentifie
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
JwtSecurityTokenHandler.DefaultOutboundClaimTypeMap.Clear();

// Emit dotnet runtime version to response header
app.Use(async (context, next) =>
{
    context.Response.Headers["DotNetVersion"] = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
    await next();
});

app.UseRouting();
app.UseCors(b =>
{
    b.AllowAnyHeader().AllowAnyMethod().AllowCredentials().SetIsOriginAllowed(_ => true);
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

app.MapHealthChecks("/health").ShortCircuit();
app.MapMetrics().ShortCircuit();
app.MapRuntimeInfo();

app.Map("/kube-env", (IKubernetesService kubernetesService) => kubernetesService.GetKubernetesEnvironment()).ShortCircuit();
app.MapGitHubWebhooks();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

using (var serviceScope = app.Services.CreateScope())
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
await app.RunAsync();
