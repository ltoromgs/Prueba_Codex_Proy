using System;
using RusticaPortal_PRMVAN.Web.Services;        // ISessionTrackerService, ApiService
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace RusticaPortal_PRMVAN.Web
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // === Antes: builder.Services ...
        public void ConfigureServices(IServiceCollection services)
        {
            // Controladores y vistas
            services.AddControllersWithViews();

            // ✅ Soporte para sesiones
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // ✅ Autenticación por cookies
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                    options.SlidingExpiration = true;
                });

            // ✅ Servicios personalizados
            services.AddSingleton<ISessionTrackerService, InMemorySessionTracker>();
            services.AddHttpClient();
            services.AddScoped<ApiService>();
        }

        // === Antes: app ...
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (!env.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // ✅ Sesiones ANTES de autenticación
            app.UseSession();

            // Middleware personalizado
            app.UseMiddleware<SingleSessionMiddleware>();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "grupovan",
                    pattern: "GrupoVan/{action=Index}/{id?}",
                    defaults: new { controller = "GrupoVan" });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
