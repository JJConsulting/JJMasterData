using System.Reflection;
using JJMasterData.Api.Handlers;
using JJMasterData.Api.Services;
using JJMasterData.Commons.Extensions;
using JJMasterData.Core.Extensions;
using JJMasterData.Swagger.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

var root = Path.Join(builder.Environment.ContentRootPath, "..","..");
var sharedSettings = Path.Combine(root, "appsettings.json");
builder.Configuration.AddJsonFile(sharedSettings).AddJsonFile("appsettings.json");
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.Formatting = Formatting.Indented;
    });

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddResponseCompression(options => { options.EnableForHttps = true; });

builder.Services.AddSwaggerGen(options =>
{
    // The magic. o:)
    options.DocumentFilter<DataDictionaryDocumentFilter>();

    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory,
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));

    options.AddSecurityDefinition("token", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Name = "token",
        Description = "API Key Authentication",
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Name = "token",
                In = ParameterLocation.Header,
                Description = "API Key Authentication",
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "token" }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddJJMasterDataCore();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();
builder.Services.AddTransient<MasterApiService>();
builder.Services.AddTransient<AccountService>();

builder.Logging.AddJJMasterDataLogger();

builder.Services.AddAuthentication(TokenAuthenticationHandler.Name)
    .AddScheme<AuthenticationSchemeOptions, TokenAuthenticationHandler>(TokenAuthenticationHandler.Name, o => { });

var app = builder.Build();

app.UseStaticFiles();

bool enableSwagger = app.Configuration.GetJJMasterData().GetSection("Swagger").GetValue<bool>("Enable");
bool enableDarkMode = app.Configuration.GetJJMasterData().GetSection("Swagger").GetValue<bool>("DarkMode");

if (enableSwagger)
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "JJMasterData API";

        options.EnablePersistAuthorization();

        if (enableDarkMode)
            options.InjectStylesheet("../css/dark-swagger.css");

        options.EnableFilter();

        options.EnableValidator();
    });
}

app.UseExceptionHandler("/Error");

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.UseJJMasterData();
app.UseSession();
app.UseResponseCompression();

app.MapControllers();

app.Run();