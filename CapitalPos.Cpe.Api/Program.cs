using CapitalPos.Cpe.Api.Infrastructure.Firma;
using CapitalPos.Cpe.Api.Infrastructure.Storage;
using CapitalPos.Cpe.Api.Infrastructure.Sunat;
using CapitalPos.Cpe.Api.Infrastructure.Xml;
using CapitalPos.Cpe.Api.Interfaces;
using CapitalPos.Cpe.Api.Middlewares;
using CapitalPos.Cpe.Api.Services;
using CapitalPos.Cpe.Api.Settings;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Settings
builder.Services.Configure<CpeSettings>(
    builder.Configuration.GetSection("CpeSettings"));

builder.Services.Configure<CpeSecuritySettings>(
    builder.Configuration.GetSection("CpeSecuritySettings"));

// Services
builder.Services.AddScoped<CpeValidacionService>();
builder.Services.AddScoped<CpeEmisionService>();
builder.Services.AddScoped<CpeConfigService>();
builder.Services.AddScoped<CatalogosService>();
builder.Services.AddScoped<CpeDiagnosticoService>();
builder.Services.AddScoped<CpeDemoService>();

// Interfaces
builder.Services.AddScoped<ICpeStorageService, CpeStorageService>();
builder.Services.AddScoped<ICpeXmlService, CpeXmlService>();
builder.Services.AddScoped<ICpeXmlValidatorService, CpeXmlValidatorService>();
builder.Services.AddScoped<ICpeXmlResumenService, CpeXmlResumenService>();
builder.Services.AddScoped<ICpeHashService, CpeHashService>();
builder.Services.AddScoped<ICpeHistorialService, CpeHistorialService>();
builder.Services.AddScoped<ICpeFirmaService, CpeFirmaService>();
builder.Services.AddScoped<ICpeSunatService, CpeSunatService>();

var app = builder.Build();

// Middlewares

app.UseMiddleware<CorrelationIdMiddleware>();

app.UseMiddleware<GlobalExceptionMiddleware>();

app.UseHttpsRedirection();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

app.Run();