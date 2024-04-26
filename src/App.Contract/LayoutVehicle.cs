namespace Tellurian.Trains.Planning.App.Contracts;
public class LayoutVehicle
{
    public required int Id { get; init; }
    public required string StartStationName { get; init; }
    public string? StartTrack { get => _startTrack; init { if (value == "0") _startTrack = null; else _startTrack = value; } }
    public string? VehicleScheduleNumber { get; init; }
    public string? OperatorSignature { get; init;  }
    public string? Class { get; init; }
    public string? VehicleNumber { get; init; }
    public string? DepartureTime { get; init; }
    public string? Note { get; init; }
    public string? OwnerName { get; init;}
    public int? LocoAddress { get; init; }
    public string? OperatingDays { get; init;}
    public int MaxNumberOfWagons { get; init; }
    public required string VehicleType { get; init; }

    private string? _startTrack;

    public bool IsLoco => VehicleType == "Loco" || VehicleType == "Railcar";
}
