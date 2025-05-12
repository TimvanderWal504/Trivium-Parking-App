using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.DTOs;

/// <summary>
/// DTO for creating parking requests insinde the API.
/// </summary>
public class CreateParkingRequestDto
{
    [Required(ErrorMessage = "Requested date is required.")]
    public DateOnly? RequestedDate { get; set; }
    [Required(ErrorMessage = "Requested country is required.")]
    public string CountryIsoCode { get; set; } = string.Empty;
    [Required(ErrorMessage = "Requested city is required.")]
    public string City { get; set; } = string.Empty;
}

