using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Middleware;
using TriviumParkingApp.Backend.Models;
using TriviumParkingApp.Backend.Repositories;
using TriviumParkingApp.Backend.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        // Register middleware - order matters!
        worker.UseMiddleware<FirebaseAuthMiddleware>();       
        // Add other middleware here if needed
    })
    .ConfigureServices((context, services) => { // Access context for configuration
        services.AddApplicationInsightsTelemetryWorkerService();

        // Configure DbContext
        var sqlConnectionString = context.Configuration.GetValue<string>("SqlAzureConnectionString");
        if (string.IsNullOrEmpty(sqlConnectionString))
        {
            throw new InvalidOperationException("SQL Azure Connection String 'SqlAzureConnectionString' not found in configuration.");
        }
        services.AddDbContextFactory<ParkingDbContext>(options =>
            options.UseSqlServer(sqlConnectionString));

        // Configure Firebase Admin SDK
        var firebaseSdkPath = context.Configuration.GetValue<string>("FirebaseAdminSdkPath");
        if (string.IsNullOrEmpty(firebaseSdkPath))
        {
            throw new InvalidOperationException("Firebase Admin SDK Path 'FirebaseAdminSdkPath' not found in configuration.");
        }
        var fullPath = Path.Combine(context.HostingEnvironment.ContentRootPath, firebaseSdkPath);
        if (!File.Exists(fullPath))
        {
             throw new FileNotFoundException($"Firebase Admin SDK file not found at: {fullPath}");
        }

        var json = File.ReadAllText(fullPath);
        using var doc = System.Text.Json.JsonDocument.Parse(json);
        var projectId = doc.RootElement.GetProperty("project_id").GetString()
                         ?? throw new InvalidOperationException("project_id niet gevonden in service account JSON");

        services.AddSingleton(FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(fullPath),
            ProjectId = projectId
        }));

        // Register HttpClientFactory
        services.AddHttpClient();

        services.AddIdentityCore<User>(options =>
        {
            options.User.RequireUniqueEmail = false;
            options.Password.RequireDigit = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireLowercase = false;
        })
        .AddRoles<Role>()
        .AddEntityFrameworkStores<ParkingDbContext>();

        services
         .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(options =>
         {
             options.Authority = $"https://securetoken.google.com/{projectId}";
             options.TokenValidationParameters = new TokenValidationParameters
             {
                 ValidateIssuer = true,
                 ValidIssuer = $"https://securetoken.google.com/{projectId}",
                 ValidateAudience = true,
                 ValidAudience = projectId,
                 ValidateLifetime = true,
             };
         });

        services.AddScoped<IClaimsTransformation, FirebaseRoleClaimsTransformation>();

        // Register Repositories (Scoped lifetime is usually appropriate)
        services.AddScoped<IParkingLotRepository, ParkingLotRepository>();
        services.AddScoped<IParkingRequestRepository, ParkingRequestRepository>();
        services.AddScoped<IAllocationRepository, AllocationRepository>();
        // TODO: Add other repository registrations (ParkingSpace)

        // Register Services (Scoped or Transient depending on state)
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IParkingLotService, ParkingLotService>();
        services.AddScoped<IParkingRequestService, ParkingRequestService>();
        services.AddScoped<IAllocationService, AllocationService>();
        // TODO: Add other service registrations (NotificationService)

    })
    .Build();

host.Run();
