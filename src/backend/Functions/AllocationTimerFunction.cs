using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TriviumParkingApp.Backend.Services; // Use Services namespace

namespace TriviumParkingApp.Backend.Functions;

public class AllocationTimerFunction
{
    private readonly ILogger<AllocationTimerFunction> _logger;
    private readonly IAllocationService _allocationService;

    public AllocationTimerFunction(
        ILogger<AllocationTimerFunction> logger,
        IAllocationService allocationService)
    {
        _logger = logger;
        _allocationService = allocationService;
    }

    [Function("RunDailyAllocation")]
    public async Task Run([TimerTrigger("0 0 3 * * 1-5"
        #if DEBUG
        , RunOnStartup = true
        #endif
    )] TimerInfo myTimer)
    {
        _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
        if (myTimer.ScheduleStatus is not null)
        {
             _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
        }

        try
        {
                            
            var allocations = await _allocationService.RunDailyAllocationAsync(DateOnly.FromDateTime(DateTime.UtcNow));

            _logger.LogInformation("Daily allocation service run completed. {Count} allocations made.", allocations?.Count() ?? 0);

            // 3. Notifications are handled within the service after saving allocations
        }
        catch (Exception ex)
        {
            // Service layer should ideally handle and log its specific errors.
            // This catch block is for errors occurring during the service call itself or date calculation.
            _logger.LogError(ex, "Error occurred during weekly allocation timer execution.");
            // Depending on the error, consider if the function should retry or fail permanently.
        }

        _logger.LogInformation($"C# Timer trigger function finished at: {DateTime.UtcNow}");
    }
}