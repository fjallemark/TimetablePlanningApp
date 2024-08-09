namespace Tellurian.Trains.Planning.App.Contracts;
public class ShuntingLoco : Vehicle
{
    public required string HomeStationName {  get; set; }
    public string? OwnerName { get; set; }
    public string? Note { get; set; }
}
