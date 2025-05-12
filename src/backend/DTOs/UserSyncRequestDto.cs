namespace TriviumParkingApp.Backend.DTOs;

/// <summary>
/// DTO for the user synchronization request body.
/// </summary>
public class UserSyncRequestDto
{
    public string FirebaseUid { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? DisplayName { get; set; }
    public string CountryIsoCode { get; set; } = null!;
}