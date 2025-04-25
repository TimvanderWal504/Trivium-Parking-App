using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using TriviumParkingApp.Backend.Services; // Use Services namespace

namespace TriviumParkingApp.Backend.Functions
{
    public class ParkingLotFunctions
    {
        private readonly ILogger<ParkingLotFunctions> _logger;
        private readonly IParkingLotService _parkingLotService; // Inject IParkingLotService

        public ParkingLotFunctions(ILogger<ParkingLotFunctions> logger, IParkingLotService parkingLotService) // Updated constructor
        {
            _logger = logger;
            _parkingLotService = parkingLotService; // Assign injected service
        }

        // TODO: Add authorization check (e.g., require logged-in user)

        [Function("GetParkingLots")]
        public async Task<HttpResponseData> GetParkingLots(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "parkingLots")] HttpRequestData req,
        FunctionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request to GetParkingLots.");

            HttpResponseData response;

            try
            {
                // Delegate to service layer, request spaces included
                var parkingLotsDto = await _parkingLotService.GetAllParkingLotsAsync(includeSpaces: true);

                response = req.CreateResponse(HttpStatusCode.OK);
                // Return the DTO list
                await response.WriteAsJsonAsync(parkingLotsDto);
            }
            catch (Exception ex) // Catch broader exceptions
            {
                _logger.LogError(ex, "Error retrieving parking lots.");
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                await response.WriteStringAsync("An error occurred while retrieving parking lots.");
            }

            return response;
        }

        // TODO: Add functions for POST, PUT, DELETE parking lots (Admin only) later using the service layer
    }
}