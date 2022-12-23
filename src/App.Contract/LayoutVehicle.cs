namespace Tellurian.Trains.Planning.App.Contracts;
public class LayoutVehicle
{
    public required int Id { get; init; }
    public required string StartStationName { get; init; }
    public string? StartTrack { get; init; }
    public string? VehicleScheduleNumber { get; init; }
    public string? OperatorSignature { get; init;  }
    public string? Class { get; init; }
    public string? VehicleNumber { get; init; }
    public string? DepartureTime { get; init; }
    public string? Note { get; init; }
    public string? OwnerName { get; init;}
    public string? OperatingDays { get; init;}

}
