using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.App.Contracts;
public class VehicleStartInfo
{
    public string? StationName { get; init; }
    public string? TrackNumber { get; init; }
    public string? OperatorSignature { get; init; }
    public int? TurnusNumber { get; init; }
    public string? VehicleClass { get; init; }
    public string? VehicleNumber { get; init; }
    public int MaxNumberOfVehicles { get; init; }
    public string? DepartureTime { get; init; }
    public required string VehicleType { get; init; }
    public short? DccAddress {  get; init; }
    public string? OwnerName { get; init; }
    public string Note {  get; init; } = string.Empty;
}

public static class VehicleStartInfoExtensions
{
    public static string DisplayedTime(this VehicleStartInfo info) =>
        info.DepartureTime.HasValue() && info.DepartureTime.Length >= 5 ? info.DepartureTime[0..5] : string.Empty;

    public static string OwnerOrNotBooked(this VehicleStartInfo info) =>
        info.OwnerName.HasValue() ? info.OwnerName : $"""<span style="color: red">EJ BOKADE</span>""";

    public static string? Notes(this VehicleStartInfo info) =>
        info.MaxNumberOfVehicles > 1 ? $"{info.Note.TrimEnd()}. {info.MaxNumberOfVehicles} vagnar." : info.Note;

    public static string DccAddressOrMissingOrNotApplicable(this VehicleStartInfo info) =>
        info.DccAddress.HasValue ? info.DccAddress.Value > 0 ? $"{info.DccAddress.Value}" : info.DccAddress.Value == 0 ? $"""<span style="color: red">SAKNAS</span>""" : "-" : "-";
}
