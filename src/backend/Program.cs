using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Repositories; // Add Repositories namespace
using TriviumParkingApp.Backend.Services; // Add Services namespace
using TriviumParkingApp.Backend.Middleware; // Add Middleware namespace

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker => // Configure worker defaults
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
        services.AddDbContext<ParkingDbContext>(options =>
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
        services.AddSingleton(FirebaseApp.Create(new AppOptions()
        {
            Credential = GoogleCredential.FromFile(fullPath),
            // Optionally add DatabaseURL or StorageBucket if needed later
        }));

        // Register HttpClientFactory
        services.AddHttpClient();

        // Register Repositories (Scoped lifetime is usually appropriate)
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRoleRepository, RoleRepository>();
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
