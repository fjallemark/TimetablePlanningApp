namespace Tellurian.Trains.Planning.App.Contracts;
public class ShuntingLoco
{
    public required string Operator { get; set; }
    public required string HomeStationName {  get; set; }
    public string? Class { get; set; } 
    public string? VehicleNumber { get; set; }
    public string? OwnerName { get; set; }
    public string? Note { get; set; }
    public bool IsRailcar { get; set; }

}
