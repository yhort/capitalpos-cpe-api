using CapitalPos.Cpe.Api.Data;
using CapitalPos.Cpe.Api.Infrastructure.Cdr;
using CapitalPos.Cpe.Api.Infrastructure.Firma;
using CapitalPos.Cpe.Api.Infrastructure.Security;
using CapitalPos.Cpe.Api.Infrastructure.Storage;
using CapitalPos.Cpe.Api.Infrastructure.Sunat;
using CapitalPos.Cpe.Api.Infrastructure.Tenancy;
using CapitalPos.Cpe.Api.Infrastructure.Xml;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Middlewares;
using CapitalPos.Cpe.Api.Services;
using CapitalPos.Cpe.Api.Settings;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

builder.Services.Configure<CpeSettings>(
    builder.Configuration.GetSection("CpeSettings"));

builder.Services.Configure<CpeSecuritySettings>(
    builder.Configuration.GetSection("CpeSecuritySettings"));

var connectionString = builder.Configuration.GetConnectionString("CpeDb")
                       ?? "Host=localhost;Port=5432;Database=CapitalPosCpeDb;Username=postgres;Password=postgres";

builder.Services.AddDbContext<CpeDbContext>(options =>
    options.UseNpgsql(connectionString));

var cpeSettings = builder.Configuration.GetSection("CpeSettings").Get<CpeSettings>() ?? new CpeSettings();
var keysPath = string.IsNullOrWhiteSpace(cpeSettings.DataProtectionKeysPath)
    ? Path.Combine(builder.Environment.ContentRootPath, "dp-keys")
    : cpeSettings.DataProtectionKeysPath;

Directory.CreateDirectory(keysPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(keysPath))
    .SetApplicationName("CapitalPos.Cpe.Api");

builder.Services.AddScoped<CpeValidacionService>();
builder.Services.AddScoped<CpeEmisionService>();
builder.Services.AddScoped<CpeConfigService>();
builder.Services.AddScoped<CatalogosService>();
builder.Services.AddScoped<CpeDiagnosticoService>();
builder.Services.AddScoped<CpeDemoService>();
builder.Services.AddScoped<IEmisorAdminService, EmisorAdminService>();

builder.Services.AddScoped<ICpeStorageService, CpeStorageService>();
builder.Services.AddScoped<ICpeXmlService, CpeXmlService>();
builder.Services.AddScoped<ICpeXmlValidatorService, CpeXmlValidatorService>();
builder.Services.AddScoped<ICpeXmlResumenService, CpeXmlResumenService>();
builder.Services.AddScoped<ICpeHashService, CpeHashService>();
builder.Services.AddScoped<ICpeHistorialService, CpeHistorialService>();
builder.Services.AddScoped<ICpeFirmaService, CpeFirmaService>();
builder.Services.AddScoped<ICpeSunatService, CpeSunatService>();
builder.Services.AddScoped<ICpeCdrService, CpeCdrService>();
builder.Services.AddScoped<ISecretProtector, DataProtectionSecretProtector>();
builder.Services.AddScoped<IEmisorRuntimeContextResolver, EmisorRuntimeContextResolver>();
builder.Services.AddScoped<IEmisorContextAccessor, EmisorContextAccessor>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Ingrese la API Key en el header X-API-KEY",
        Name = "X-API-KEY",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddHttpClient();
var app = builder.Build();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<CpeDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.MapControllers();
app.Run();

public partial class Program
{
}
