using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using System;
using System.Globalization;

namespace MVC.SOIMSF
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Agregar servicios al contenedor
            builder.Services.AddControllersWithViews();

            // Configuraci�n de soporte para sesiones
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30); 
                options.Cookie.HttpOnly = true; 
                options.Cookie.IsEssential = true; 
                options.Cookie.SameSite = SameSiteMode.Strict;  
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always; 
            });

            // Configuraci�n de autenticaci�n basada en cookies
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Authentication/Login";
                    options.LogoutPath = "/Authentication/Logout";
                    options.AccessDeniedPath = "/Authentication/AccessDenied";
                });

            builder.Services.AddAuthorization();

            // Acceso a contexto HTTP
            builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Registrar un cliente HTTP con configuraci�n personalizada para llamadas a la API
            builder.Services.AddHttpClient("ApiService", client =>
            {
                client.BaseAddress = new Uri("https://soimsf.somee.com/");
            });

            var app = builder.Build();

            // Configuraci�n del pipeline HTTP
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Middleware para sesiones
            app.UseSession();

            // Middleware para autenticaci�n y autorizaci�n
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}