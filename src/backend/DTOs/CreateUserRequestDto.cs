using System.ComponentModel.DataAnnotations;

namespace TriviumParkingApp.Backend.DTOs;

/// <summary>
/// DTO for the request body when creating a new user.
/// </summary>
public class CreateUserRequestDto
{
    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Password is required.")]
    [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
    public string Password { get; set; } = null!;

    [StringLength(100, ErrorMessage = "Display name cannot exceed 100 characters.")]
    public string? DisplayName { get; set; }

    [Required(ErrorMessage = "CountryCode is required.")]
    [StringLength(2, ErrorMessage = "CountryCode must only be 2 characters long.")]
    public string CountryIsoCode { get; set; } = null!;
}