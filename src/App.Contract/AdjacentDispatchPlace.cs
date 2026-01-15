namespace Tellurian.Trains.Planning.App.Contracts;

public class AdjacentDispatchPlace
{
    public required string Name { get; init; }
    public int LayoutStationId { get; init; }
    public string? PhoneNumber { get; init; }
    public string? PhoneNote { get; init; }

    public override string ToString() => $"{Name}: {PhoneNumber}";
}
