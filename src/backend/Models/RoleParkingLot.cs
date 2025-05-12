namespace TriviumParkingApp.Backend.Models;

public class RoleParkingLot
{
    public int RoleId { get; set; }
    public virtual Role Role { get; set; } = null!;

    public int ParkingLotId { get; set; }
    public virtual ParkingLot ParkingLot { get; set; } = null!;
}
