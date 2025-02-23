using System.Text;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.App.Contracts;
public class VehicleStartInfo
{
    public string? StationName { get; init; }
    public string? TrackNumber { get; init; }
    public bool IsFirstDay { get; init; }
    public int LayoutStartWeekday {  get; init; }
    public byte DayFlags { get; init; }
    public string? OperatorSignature { get; init; }
    public int? TurnusNumber { get; init; }
    public string? VehicleClass { get; init; }
    public string? VehicleNumber { get; init; }
    public int MaxNumberOfVehicles { get; init; }
    public string? DepartureTime { get; init; }
    public required string VehicleType { get; init; }
    public short? DccAddress { get; init; }
    public string? OwnerName { get; init; }
    public string Note { get; init; } = string.Empty;
    public int ReplaceOrder { get; init; }
    public bool HasFredThrottle { get; init; }
    public int Position { get; set; }
    public required string Type { get; set; }
    public override string ToString() => $"{OperatorSignature} {VehicleClass} {VehicleNumber} {DayFlags.OperationDays().ShortName} {LayoutStartWeekday}";
}

public static class VehicleStartInfoExtensions
{
    public static string FirstOperationDay(this VehicleStartInfo info) =>
        info.DayFlags.FirstOperationDay(info.LayoutStartWeekday == 7).FullName;
    public static string DisplayedTime(this VehicleStartInfo info) =>
        info.DepartureTime.HasValue() && info.DepartureTime.Length >= 5 ? info.DepartureTime[0..5] : string.Empty;

    public static string OwnerOrNotBooked(this VehicleStartInfo info) =>
        info.OwnerName.HasValue() ? info.OwnerName : info.MaxNumberOfVehicles==0 ? "" : $"""<span style="color: red">{Resources.Notes.NotBooked.ToUpperInvariant()}</span>""";

    public static string Notes(this VehicleStartInfo info)
    {
        var text = new StringBuilder(100);
        if (info.ReplaceOrder == 1)
        {
            text.Append(Resources.Notes.AdditionalVehicle);
            text.Append(". ");

        }
        else if (info.ReplaceOrder > 0)
        {
            if(info.ReplaceOrder < 9) text.Append(Resources.Notes.SpareVehicle);
            else text.Append(Resources.Notes.OtherVehicle);
            text.Append(". ");
        }
        if (info.Note.HasValue())
        {
            text.Append(info.Note);
            text.Append(' ');
        }
        if (info.MaxNumberOfVehicles > 1)
        {
            text.Append(string.Format(Resources.Notes.NumberOfWagons, info.MaxNumberOfVehicles));
        }
        return text.ToString();
    }

    public static string FredYesNo(this VehicleStartInfo info) => info.DccAddress.HasValue ? info.HasFredThrottle ? Resources.Notes.Yes : Resources.Notes.No : "-";


    public static string DccAddressOrMissingOrNotApplicable(this VehicleStartInfo info) =>
        info.Type.AnyOf(["Loco", "Shunter", "Railcar"]) && info.OwnerName.HasValue() ?
        info.DccAddress > 0 ? $"{info.DccAddress.Value}" :  
        $"""<span style="color: red">{Resources.Notes.Missing.ToUpperInvariant()}</span>""" :
        "-" ;

    public static string LocoNumberOrMissingOrWagonNumber(this VehicleStartInfo info) =>
        info.Type.AnyOf(["Loco", "Shunter", "Railcar"]) && info.OwnerName.HasValue() ?
        info.VehicleNumber.HasValue() ? $"{info.VehicleNumber}" :
        $"""<span style="color: red">{Resources.Notes.Missing.ToUpperInvariant()}</span>""" :
        info.VehicleNumber ?? "-";


    public static string BackColor(this VehicleStartInfo info) =>
        info.ReplaceOrder == 0 && !info.IsFirstDay && info.DayFlags > 0 && info.DayFlags < 127? "lightyellow": info.ReplaceOrder == 9 ? "gainsboro": info.ReplaceOrder > 1 ? "gainsboro" : string.Empty;
}
