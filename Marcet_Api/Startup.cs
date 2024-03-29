﻿using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Marcet_Api.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Marcet_Log.ILogger;
using Serilog;
using Marcet_DB.Sevice;

namespace Marcet_DB
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            Log.Logger = new LoggerConfiguration()
               .ReadFrom.Configuration(Configuration)
               .CreateLogger();

            services.AddSingleton(Log.Logger); // Register Serilog logger
            services.AddSingleton<ILoggerService, SerilogLoggerService>();
            // Добавление строки подключения
            string connection = "Server=192.168.0.101;Database=Marcet_DB;User Id=sa;Password=vopenu48;TrustServerCertificate=True";

            services.AddControllers();

            // Подключение к базе данных
            services.AddDbContext<ApplicationContext>(options =>
                options.UseSqlServer(connection));

            // Регистрация сервиса корзины
            services.AddScoped<ICartService, CartService>(); // Замените на фактический тип вашего сервиса корзины

            // Регистрация сервиса заказов
            services.AddScoped<IOrderService, OrderService>(); // Замените на фактический тип вашего сервиса заказов

            // Регистрация Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API", Version = "v1" });

                // Добавьте аутентификацию JWT для Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] { }
                    }
                });
            });

            // Настройка аутентификации с использованием JWT
            var authOptions = Configuration.GetSection("AuthOptions").Get<AuthOptions>();
            var securityKey = authOptions.GetSymmetricSecurityKey();
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false; // Установите true в продакшене для HTTPS
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = authOptions.Issuer,
                        ValidateAudience = true,
                        ValidAudience = authOptions.Audience,
                        ValidateLifetime = true,
                        IssuerSigningKey = securityKey,
                        ValidateIssuerSigningKey = true,
                    };
                });

            // Добавление сервисов для аутентификации и авторизации
            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
                options.AddPolicy("User", policy => policy.RequireRole("User"));
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                // Используйте Swagger с аутентификацией
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");

                    // Настройка аутентификации для Swagger UI
                    c.OAuthClientId("swagger-ui");
                    c.OAuthClientSecret("swagger-ui-secret");
                    c.OAuthUseBasicAuthenticationWithAccessCodeGrant();
                });
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }
            //ErrorHanding
            app.UseMiddleware<ErrorHandlingMiddleware>();
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Включение аутентификации и авторизации
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
