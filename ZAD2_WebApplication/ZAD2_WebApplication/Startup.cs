﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ZAD2_WebApplication.Controllers;
using ZAD2_WebApplication.DAL;
using ZAD2_WebApplication.Middlewares;

namespace ZAD2_WebApplication
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IStudentsDbService, SqlServerDbService>();

            // Dodanie autoryzacji używając JSON Web Token
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidIssuer = "GakkoDB",
                        ValidAudience = "All",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["SecretKey"]))
                    };
                });

            services.AddControllers()
                .AddXmlSerializerFormatters();

            // Dodanie generatora dokumentacji - bliblioteka Swagger
            services.AddSwaggerGen(config =>
            {
                config.SwaggerDoc("v1", new OpenApiInfo { Title = "My_C-Szarp_App", Version = "v1" });
            });
            
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Middleware dev. logowanie błędów itp.
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            // Middleware dokumentacji
            app.UseSwagger();
            app.UseSwaggerUI(config =>
            {
                config.SwaggerEndpoint("/swagger/v1/swagger.json", "My_C-Szarp_App");
            });

            // Middleware logujący informacje do pliku
            app.UseMiddleware<LoggingMiddleware>();

            // Middleware weryfikujący numer indexu od autora przychodzącego żądania
            app.UseMiddleware<ValidateStudentRequestPermission>();

            // Middleware przyjmujący żądania przychodzące na API i kerujący je dalej do obsługi
            app.UseRouting();

            // Middleware autentykacji - czy jesteś tym kim jesteś
            app.UseAuthentication();

            // Middleware autoryzacji - czy masz dostęp do określonych zasobów
            app.UseAuthorization();

            // Middleware kierujący żądanie już do właściwej obsługi
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
