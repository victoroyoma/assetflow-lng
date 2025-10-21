using buildone.Authorization;
using buildone.Data;
using buildone.Middleware;
using buildone.Services;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Reflection;

// Configure Serilog for structured logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "BuildOne")
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/buildone-.log",
        rollingInterval: RollingInterval.Day,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
        retainedFileCountLimit: 30)
    .CreateLogger();

try
{
    Log.Information("Starting BuildOne Asset Management System");

    var builder = WebApplication.CreateBuilder(args);

    // Use Serilog for logging
    builder.Host.UseSerilog();

    // Add services to the container.
    builder.Services.AddRazorPages();
    builder.Services.AddControllersWithViews(); // Add MVC support with views

    // Add FluentValidation
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();
    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    // Add AutoMapper
    builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());

    // Add Swagger/OpenAPI support
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "BuildOne Asset Management API",
            Version = "v1",
            Description = "API for managing assets, employees, departments, and imaging jobs",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "BuildOne Support",
                Email = "support@buildone.com"
            }
        });

        // Enable XML comments if available
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

// Database configuration with retry policy
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null));
});

// Configure Identity with enhanced security
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Enhanced password requirements
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;
    options.Password.RequiredUniqueChars = 3;
    
    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
    
    // User settings
    options.User.AllowedUserNameCharacters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;
    
    // Sign in settings
    options.SignIn.RequireConfirmedAccount = false;
    options.SignIn.RequireConfirmedEmail = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Configure cookie authentication with enhanced security
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;
});

// Configure Authorization Policies
builder.Services.AddAuthorization(options =>
{
    // User Management Policies
    options.AddPolicy(Policies.CanManageUsers, policy =>
        policy.RequireRole(Roles.Administrator));
    options.AddPolicy(Policies.CanViewUsers, policy =>
        policy.RequireRole(Roles.Administrator, Roles.Technician));

    // Asset Management Policies
    options.AddPolicy(Policies.CanManageAssets, policy =>
        policy.RequireRole(Roles.Administrator, Roles.Technician));
    options.AddPolicy(Policies.CanViewAssets, policy =>
        policy.RequireAuthenticatedUser());
    options.AddPolicy(Policies.CanAssignAssets, policy =>
        policy.RequireRole(Roles.Administrator, Roles.Technician));

    // Department Management Policies
    options.AddPolicy(Policies.CanManageDepartments, policy =>
        policy.RequireRole(Roles.Administrator));
    options.AddPolicy(Policies.CanViewDepartments, policy =>
        policy.RequireAuthenticatedUser());

    // Employee Management Policies
    options.AddPolicy(Policies.CanManageEmployees, policy =>
        policy.RequireRole(Roles.Administrator));
    options.AddPolicy(Policies.CanViewEmployees, policy =>
        policy.RequireAuthenticatedUser());

    // Imaging Jobs Policies
    options.AddPolicy(Policies.CanManageImagingJobs, policy =>
        policy.RequireRole(Roles.Administrator, Roles.Technician));
    options.AddPolicy(Policies.CanViewImagingJobs, policy =>
        policy.RequireAuthenticatedUser());
    options.AddPolicy(Policies.CanPerformImaging, policy =>
        policy.RequireRole(Roles.Technician));

    // Role Management Policies
    options.AddPolicy(Policies.CanManageRoles, policy =>
        policy.RequireRole(Roles.Administrator));
    options.AddPolicy(Policies.CanViewRoles, policy =>
        policy.RequireRole(Roles.Administrator));

    // System Settings Policies
    options.AddPolicy(Policies.CanAccessSystemSettings, policy =>
        policy.RequireRole(Roles.Administrator));
    options.AddPolicy(Policies.CanModifySystemSettings, policy =>
        policy.RequireRole(Roles.Administrator));

    // Reports Policies
    options.AddPolicy(Policies.CanViewReports, policy =>
        policy.RequireAuthenticatedUser());
    options.AddPolicy(Policies.CanExportReports, policy =>
        policy.RequireRole(Roles.Administrator, Roles.Technician));
});

// Register authorization handlers
builder.Services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

// Add services
builder.Services.AddScoped<IAssetService, AssetService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IImagingJobService, ImagingJobService>();
builder.Services.AddScoped<IAssetHistoryService, AssetHistoryService>();
builder.Services.AddScoped<IDataSeedingService, DataSeedingService>();
builder.Services.AddScoped<IBulkDataSeedingService, BulkDataSeedingService>();
builder.Services.AddScoped<IJobCommentService, JobCommentService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IExportService, ExportService>();
builder.Services.AddMemoryCache();

// Add health checks with detailed diagnostics
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>(
        name: "database",
        tags: new[] { "db", "sql", "sqlserver" });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // Use global exception handler middleware
    app.UseGlobalExceptionHandler();
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
    
    // Enable Swagger in development and staging
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "BuildOne Asset Management API v1");
        c.RoutePrefix = "swagger";
        c.DocumentTitle = "BuildOne API Documentation";
        c.DisplayRequestDuration();
    });
}

// Add request logging middleware
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserName", httpContext.User.Identity?.Name ?? "Anonymous");
    };
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// Map MVC routes first (more specific)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Enhanced health check endpoints
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };
        await context.Response.WriteAsJsonAsync(response);
    }
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db")
});

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var seedingService = scope.ServiceProvider.GetRequiredService<IDataSeedingService>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    
    try
    {
        // Test database connection
        if (app.Environment.IsDevelopment())
        {
            await context.Database.EnsureCreatedAsync();
        }
        
        // Seed initial data (roles and admin user)
        await seedingService.SeedDataAsync();
        
        logger.LogInformation("Database initialized successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database initialization failed");
        throw; // Re-throw in production to prevent startup with broken database
    }
}

    Log.Information("BuildOne started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application failed to start");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
