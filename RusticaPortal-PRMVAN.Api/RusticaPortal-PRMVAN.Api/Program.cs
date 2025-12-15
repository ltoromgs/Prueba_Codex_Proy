using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace RusticaPortal_PRMVAN.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // ----------------------
            // Serilog configuration
            // ----------------------
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.File("Logs/info.log",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information,
                    rollingInterval: RollingInterval.Day)
                .WriteTo.File("Logs/warning.log",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning,
                    rollingInterval: RollingInterval.Day)
                .WriteTo.File("Logs/error.log",
                    restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error,
                    rollingInterval: RollingInterval.Day)
                .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseSerilog() // usa la configuración de arriba
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}


//using RusticaPortal_PRMVAN.Api.Automapper;
//using RusticaPortal_PRMVAN.Api.Entities.Dto;
//using RusticaPortal_PRMVAN.Api.Entities.ObjectSAP;
//using RusticaPortal_PRMVAN.Api.Services;
//using RusticaPortal_PRMVAN.Api.Services.Interfaces;
//using RusticaPortal_PRMVAN.Api.Swagger;
//using Microsoft.Extensions.Options;
//using Serilog;

//var configuration = new ConfigurationBuilder()
//    .SetBasePath(Directory.GetCurrentDirectory())
//    .AddJsonFile("appsettings.json", optional: false)
//    .Build();

//// ----------------------
//// Serilog configuration
//// ----------------------
//Log.Logger = new LoggerConfiguration()
//    .MinimumLevel.Verbose()
//    .WriteTo.File("Logs/info.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day)
//    .WriteTo.File("Logs/warning.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning, rollingInterval: RollingInterval.Day)
//    .WriteTo.File("Logs/error.log", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error, rollingInterval: RollingInterval.Day)
//    .CreateLogger();

//var builder = WebApplication.CreateBuilder(args);
//builder.Host.UseSerilog();

//// ------------------------------------
//// Register our configuration instances
//// ------------------------------------
//builder.Services.AddSingleton<IConfiguration>(configuration);

//// Bind the "Empresas" section into a list of EmpresaConfig
//builder.Services.Configure<List<EmpresaConfig>>(configuration.GetSection("Empresas"));
//// Expose a service to look up EmpresaConfig by ID
//builder.Services.AddSingleton<IEmpresaConfigService, EmpresaConfigService>();

//// ----------------------
//// MVC / API registrations
//// ----------------------
//builder.Services.AddControllers();
//builder.Services.AddMemoryCache();
//builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen(o => o.SchemaFilter<SwaggerIgnoreFilter>());
//builder.Services.AddAutoMapper(typeof(AutoMapperProfile));

//// ----------------------
//// Application services
//// ----------------------
//builder.Services.AddScoped<ILoginService, LoginService>();
//builder.Services.AddScoped<IDocumentService, DocumentService>();

//var app = builder.Build();

//// ----------------------
//// Middleware pipeline
//// ----------------------
//app.UseSwagger();
//app.UseSwaggerUI();

//app.UseHttpsRedirection();
//app.UseAuthorization();
//app.MapControllers();

//app.Run();



