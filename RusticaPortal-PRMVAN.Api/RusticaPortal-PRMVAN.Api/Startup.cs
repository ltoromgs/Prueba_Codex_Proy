using System.Collections.Generic;
using RusticaPortal_PRMVAN.Api.Automapper;
using RusticaPortal_PRMVAN.Api.Entities.Dto;
using RusticaPortal_PRMVAN.Api.Entities.ObjectSAP;
using RusticaPortal_PRMVAN.Api.Services;
using RusticaPortal_PRMVAN.Api.Services.Interfaces;
using RusticaPortal_PRMVAN.Api.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace RusticaPortal_PRMVAN.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddMemoryCache();

           // services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(o =>
            {
                o.SchemaFilter<SwaggerIgnoreFilter>();
            });

            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IDocumentService, DocumentService>();
            services.AddScoped<IEmpresaRuntimeService, EmpresaRuntimeService>();


            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });

            // empresa dinámica
            services.Configure<List<EmpresaConfig>>(Configuration.GetSection("Empresas"));
            services.AddSingleton<IEmpresaConfigService, EmpresaConfigService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}