using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;
using TriviumParkingApp.Backend.Attributes;
using TriviumParkingApp.Backend.Data;
using TriviumParkingApp.Backend.Middleware;
using TriviumParkingApp.Backend.Repositories;
using TriviumParkingApp.Backend.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        // Register middleware - order matters!
        worker.UseMiddleware<FirebaseAuthMiddleware>();
        worker.UseWhen<AuthenticationMiddleware>(context =>
        {
            var entry = context.FunctionDefinition.EntryPoint;
            var lastDot = entry.LastIndexOf('.');
            var className = entry.Substring(0, lastDot);
            var methodName = entry[(lastDot + 1)..];

            var asm = Assembly.GetExecutingAssembly();
            var type = asm.GetType(className);
            var method = type?.GetMethod(methodName);

            // Kijk of method of class voorzien is van ons auth-attribute
            bool onMethod = method?.GetCustomAttribute<RequiresAuthenticationMiddlewareAttribute>(inherit: true) != null;
            bool onClass = type?.GetCustomAttribute<RequiresAuthenticationMiddlewareAttribute>(inherit: true) != null;

            return onMethod || onClass;
        });
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
