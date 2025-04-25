using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using TriviumParkingApp.Backend.Services; // Use Services namespace

namespace TriviumParkingApp.Backend.Functions
{
    public class AllocationTimerFunction
    {
        private readonly ILogger<AllocationTimerFunction> _logger;
        private readonly IAllocationService _allocationService; // Inject Service

        public AllocationTimerFunction(
            ILogger<AllocationTimerFunction> logger,
            IAllocationService allocationService) // Updated constructor
        {
            _logger = logger;
            _allocationService = allocationService; // Assign injected service
        }

        // Timer schedule (NCRONTAB format): Runs once a week, e.g., Sunday at 8 PM UTC
        // Example: "0 0 20 * * Sun"
        [Function("RunWeeklyAllocation")]
        public async Task Run([TimerTrigger("0 0 20 * * Sun", RunOnStartup = false)] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.UtcNow}");
            if (myTimer.ScheduleStatus is not null)
            {
                 _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }

            try
            {
                // 1. Determine Date Range for Allocation (e.g., next Monday to Friday)
                // TODO: Make this logic more robust (handle holidays, config?)
                DateTime today = DateTime.UtcNow.Date;
                DayOfWeek currentDayOfWeek = today.DayOfWeek;
                int daysUntilMonday = ((int)DayOfWeek.Monday - (int)currentDayOfWeek + 7) % 7;
                if (daysUntilMonday == 0) daysUntilMonday = 7; // Target next Monday if today is Monday

                DateOnly startOfWeek = DateOnly.FromDateTime(today.AddDays(daysUntilMonday));
                DateOnly endOfWeek = startOfWeek.AddDays(4); // Assuming Mon-Fri

                // 2. Delegate allocation logic to the service
                var allocations = await _allocationService.RunWeeklyAllocationAsync(startOfWeek, endOfWeek);

                _logger.LogInformation("Weekly allocation service run completed. {Count} allocations made.", allocations?.Count() ?? 0);

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
}