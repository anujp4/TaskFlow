using Azure.Identity;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Text;
using TaskFlow.API.Middleware;
using TaskFlow.Application.Interfaces;
using TaskFlow.Application.Validators;
using TaskFlow.Core.Entities;
using TaskFlow.Core.Interfaces;
using TaskFlow.Infrastructure.Data;
using TaskFlow.Infrastructure.Repositories;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .CreateLogger();

try
{
    Log.Information("Starting TaskFlow API");

    var builder = WebApplication.CreateBuilder(args);

    // Configure Key Vault (for Azure deployment)
    if (builder.Environment.IsProduction())
    {
        var keyVaultName = builder.Configuration["KeyVaultName"];
        if (!string.IsNullOrEmpty(keyVaultName))
        {
            var keyVaultUri = new Uri($"https://{keyVaultName}.vault.azure.net/");
            builder.Configuration.AddAzureKeyVault(
                keyVaultUri,
                new DefaultAzureCredential());

            Log.Information("Key Vault configured: {KeyVaultName}", keyVaultName);
        }
    }

    // Add Serilog
    builder.Host.UseSerilog();

    // Add Application Insights
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
    });

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Configure API Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.SubstituteApiVersionInUrl = true;
    });

    // Configure Swagger with JWT
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = "TaskFlow API",
            Version = "v1",
            Description = "Enterprise Task Management System with AI Integration",
            Contact = new OpenApiContact
            {
                Name = "TaskFlow Team",
                Email = "support@taskflow.com"
            }
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter 'Bearer' [space] and then your token"
        });

        options.AddSecurityRequirement(new OpenApiSecurityRequirement
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

    // Configure Database
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        options.UseSqlServer(connectionString,
            sqlOptions =>
            {
                sqlOptions.MigrationsAssembly("TaskFlow.Infrastructure");
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
            });
    });

    // Configure Identity
    builder.Services.AddIdentity<User, IdentityRole>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

    // Configure JWT Authentication
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");
    var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret not configured");

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
        };
    });

    // Register repositories
    builder.Services.AddScoped<ITaskRepository, TaskRepository>();
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

    // Register AutoMapper
    builder.Services.AddAutoMapper(typeof(TaskFlow.Application.Mappings.MappingProfile));

    // Register Application Services
    builder.Services.AddScoped<IAuthService, TaskFlow.Application.Services.AuthService>();
    builder.Services.AddScoped<ITaskService, TaskFlow.Application.Services.TaskService>();

    // Register FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddValidatorsFromAssemblyContaining<RegisterDtoValidator>();

    // Configure CORS
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // Health checks
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API V1");
        });
    }
    else
    {
        // Enable Swagger in production too (you can remove this if you want)
        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "TaskFlow API V1");
            c.RoutePrefix = string.Empty; // Serve Swagger at root in production
        });
    }

    // Add custom middleware
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();
    app.MapHealthChecks("/health");

    // Run migrations automatically on startup (Azure deployment)
    if (app.Environment.IsProduction())
    {
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            try
            {
                Log.Information("Running database migrations...");
                dbContext.Database.Migrate();
                Log.Information("Database migrations completed successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An error occurred while migrating the database");
            }
        }
    }

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
}
finally
{
    Log.CloseAndFlush();
}