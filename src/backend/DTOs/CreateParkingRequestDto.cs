using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.DTOs;

/// <summary>
/// DTO for creating parking requests insinde the API.
/// </summary>
public class CreateParkingRequestDto
{
    [Required(ErrorMessage = "Requested date is required.")]
    public DateOnly? RequestedDate { get; set; }
}

