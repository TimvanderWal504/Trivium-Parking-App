namespace TriviumParkingApp.Backend.DTOs
{
    /// <summary>
    /// DTO for the user synchronization request body.
    /// IMPORTANT: In production, details should come from validated token, not body.
    /// </summary>
    public class UserSyncRequestDto
    {
        public string? FirebaseUid { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
    }
}