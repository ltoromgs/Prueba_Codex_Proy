using System.Collections.Generic;
using ArellanoCore.Api.Automapper;
using ArellanoCore.Api.Entities.Dto;
using ArellanoCore.Api.Entities.ObjectSAP;
using ArellanoCore.Api.Services;
using ArellanoCore.Api.Services.Interfaces;
using ArellanoCore.Api.Swagger;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


namespace ArellanoCore.Api
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
            // ----------------------
            // Register configuration
            // ----------------------
            services.AddSingleton<IConfiguration>(Configuration);

            services.Configure<List<EmpresaConfig>>(Configuration.GetSection("Empresas"));
            services.AddSingleton<IEmpresaConfigService, EmpresaConfigService>();

            // ----------------------
            // MVC / API
            // ----------------------
            services.AddControllers();
            services.AddMemoryCache();
            //services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(o => o.SchemaFilter<SwaggerIgnoreFilter>());
            services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<AutoMapperProfile>();
            });

            // ----------------------
            // Application services
            // ----------------------
            services.AddScoped<ILoginService, LoginService>();
            services.AddScoped<IDocumentService, DocumentService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Swagger
            app.UseSwagger();
            app.UseSwaggerUI();

            if (env.IsDevelopment() == false)
            {
                app.UseHttpsRedirection();
            }

            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
