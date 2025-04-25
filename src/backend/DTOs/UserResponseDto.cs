namespace TriviumParkingApp.Backend.DTOs
{
    /// <summary>
    /// DTO for returning user details via the API.
    /// </summary>
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string FirebaseUid { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}