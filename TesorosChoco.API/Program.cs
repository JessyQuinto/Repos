using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using TesorosChoco.Infrastructure;
using TesorosChoco.Application;
using TesorosChoco.API.Middlewares;
using Serilog;
using FluentValidation.AspNetCore;
using System.Text.Json;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting TesorosChoco API");    
    var builder = WebApplication.CreateBuilder(args);

    // Add Serilog for structured logging
    builder.Host.UseSerilog();
    
    // Add services to the container
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.JsonSerializerOptions.WriteIndented = true;
        });

    // Add Problem Details (para manejo de errores)
    builder.Services.AddProblemDetails(options =>
    {
        options.CustomizeProblemDetails = (context) =>
        {
            context.ProblemDetails.Instance = context.HttpContext.Request.Path;
            context.ProblemDetails.Extensions.TryAdd("traceId", context.HttpContext.TraceIdentifier);
            
            // En producción, no exponer detalles técnicos
            if (!builder.Environment.IsDevelopment())
            {
                context.ProblemDetails.Detail = "An error occurred processing your request.";
            }
        };
    });
    
    // Add FluentValidation
    builder.Services.AddFluentValidationAutoValidation()
                    .AddFluentValidationClientsideAdapters();

    // Add Infrastructure and Application layers
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddApplication();
    
    // Add API versioning (simplified)
    builder.Services.AddApiVersioning(opt =>
    {
        opt.DefaultApiVersion = new ApiVersion(1, 0);
        opt.AssumeDefaultVersionWhenUnspecified = true;
    });

    builder.Services.AddVersionedApiExplorer(setup =>
    {
        setup.GroupNameFormat = "'v'VVV";
        setup.SubstituteApiVersionInUrl = true;
    });
    
    // Add CORS - Mejorado con configuración más segura
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowFrontend", policy =>
        {
            if (builder.Environment.IsDevelopment())
            {
                policy.WithOrigins(
                        "http://localhost:3000", "https://localhost:3000", // React app URLs
                        "http://localhost:8080", "https://localhost:8080", // Vite default URLs
                        "http://localhost:5173", "https://localhost:5173"  // Vite alternative URLs
                      )
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            }
            else
            {
                // En producción, especificar dominios exactos
                policy.WithOrigins("https://yourdomain.com")
                      .WithHeaders("Content-Type", "Authorization")
                      .WithMethods("GET", "POST", "PUT", "DELETE")
                      .AllowCredentials();
            }
        });
    });

    // Add Security Headers
    builder.Services.AddHsts(options =>
    {
        options.Preload = true;
        options.IncludeSubDomains = true;
        options.MaxAge = TimeSpan.FromDays(365);
    });

    // Add Swagger/OpenAPI
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo 
        { 
            Title = "TesorosChoco API", 
            Version = "v1",
            Description = "API para la gestión de productos de chocolate artesanal"
        });
        
        // Add JWT Authentication to Swagger
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
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
                Array.Empty<string>()
            }
        });
    });

    var app = builder.Build();

    // Ensure database is created and seeded
    // await app.Services.EnsureDatabaseCreatedAsync();
    
    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TesorosChoco API v1");
            c.RoutePrefix = string.Empty; // Swagger available at root
        });
    }
    else
    {
        app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
        app.UseHsts();
        
        // Agregar cabeceras de seguridad adicionales
        app.Use(async (context, next) =>
        {
            context.Response.Headers["X-Content-Type-Options"] = "nosniff";
            context.Response.Headers["X-Frame-Options"] = "DENY";
            context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
            context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
            await next();
        });
    }

    // Forzar HTTPS en producción
    if (!app.Environment.IsDevelopment())
    {
        app.UseHttpsRedirection();
    }
    
    app.UseCors("AllowFrontend");
    
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("TesorosChoco API started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}