using System.Text;
using Tellurian.Trains.Planning.App.Contracts.Extensions;

namespace Tellurian.Trains.Planning.App.Contracts;
public class VehicleStartInfo
{
    public int Id { get; set; }
    public string? StationName { get; init; }
    public string? TrackNumber { get; init; }
    public bool IsFirstDay { get; init; }
    public int LayoutStartWeekday { get; init; }
    public byte DayFlags { get; init; }
    public string? OperatorName { get; init; }
    public int? TurnusNumber { get; init; }
    public int TrainCategoryId { get; init; }
    public string TrainPrefix { get; set; } = "";
    public int TrainNumber { get; init; }
    public string? VehicleClass { get; init; }
    public string? VehicleNumber { get; init; }
    public int MaxNumberOfVehicles { get; init; }
    public string? DepartureTime { get; init; }
    public required string VehicleType { get; init; }
    public short? DccAddress { get; init; }
    public string? OwnerName { get; init; }
    public string Note { get; init; } = string.Empty;
    public int ReplaceOrder { get; init; }
    public bool PrintCard { get; init; }
    public bool HasFredThrottle { get; init; }
    [Obsolete("Use booking id")]
    public int Position { get; set; }
    public string? BookingId { get; set; }
    public required string Type { get; set; }
    public override string ToString() => $"{OperatorName} {VehicleClass} {VehicleNumber} {DayFlags.OperationDays().ShortName} {LayoutStartWeekday}";
}

public static class VehicleStartInfoExtensions
{
    public static string TrainLabel(this VehicleStartInfo info) =>
        info.TrainNumber == 0 ? "-" :
        $"{info.TrainPrefix}{info.TrainNumber}";

    public static string TrackLabel(this VehicleStartInfo info) =>
        info.TrackNumber.HasValue() ? info.TrackNumber : "-";

    public static string VehicleClassLabel(this VehicleStartInfo info) =>
        info.VehicleClass == "?" ?
            info.OwnerName.HasValue() ? Highlight(Resources.Notes.Missing) : Resources.Notes.Optional :
        info.VehicleClass ?? "-";

    public static string BookingId(this VehicleStartInfo info) =>
        info.IsOther() ? $"X{info.Id}" :
        $"{info.Type[..1]}{info.Id}";
    public static string FirstOperationDay(this VehicleStartInfo info) =>
        info.DayFlags.FirstOperationDay(info.LayoutStartWeekday == 7).FullName;
    public static string DisplayedTime(this VehicleStartInfo info) =>
        info.DepartureTime.HasValue() && info.DepartureTime.Length >= 5 ? info.DepartureTime[0..5] : "-";

    public static string OwnerOrNotBooked(this VehicleStartInfo info) =>
        info.OwnerName.HasValue() ? info.OwnerName : info.MaxNumberOfVehicles == 0 ? "" : Highlight(Resources.Notes.NotBooked);

    public static string Highlight(string? text) =>
        string.IsNullOrWhiteSpace(text) ? string.Empty :
        $"""<span style="color: red">{text}</span>""";

    public static string Notes(this VehicleStartInfo info)
    {
        var text = new StringBuilder(100);
        if (info.IsExtra())
        {
            text.Append(Resources.Notes.AdditionalVehicle);
            text.Append(". ");
        }
        else if (info.IsSpare())
        {
            text.Append(Resources.Notes.SpareVehicle);
            text.Append(". ");
        }
        else if (info.IsOther())
        {
            text.Append(Resources.Notes.OtherVehicle);
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

    public static bool IsOrdinary(this VehicleStartInfo info) => info.ReplaceOrder == 0;
    private static bool IsExtra(this VehicleStartInfo info) => info.ReplaceOrder == 1;
    private static bool IsSpare(this VehicleStartInfo info) => info.ReplaceOrder > 1 && info.ReplaceOrder < 9;
    private static bool IsOther(this VehicleStartInfo info) => info.ReplaceOrder == 9;
    private static bool IsCardRequired(this VehicleStartInfo info) => info.PrintCard == false;

    public static bool IsVehicleWithDccAddress(this VehicleStartInfo info) =>
        info.Type.AnyOf(["Loco", "Shunter", "Railcar"]);
    public static string FredYesNo(this VehicleStartInfo info) =>
        info.IsVehicleWithDccAddress() ?
            info.OwnerName.HasValue() ? info.HasFredThrottle ? Resources.Notes.Yes : Highlight(Resources.Notes.No) : "?" : "-";

    public static string CardYesNo(this VehicleStartInfo info) =>
        info.IsCardRequired() ? Resources.Notes.Yes : Resources.Notes.No;

    public static bool HasIllegalDccAddress(this VehicleStartInfo info) => info.DccAddress == 3;

    public static string DccAddressOrMissingOrNotApplicable(this VehicleStartInfo info) =>
        info.Type.AnyOf(["Loco", "Shunter", "Railcar"]) && info.OwnerName.HasValue() ?
        info.HasIllegalDccAddress() ? Highlight($"{info.DccAddress}§") :
        info.DccAddress > 0 ? $"{info.DccAddress.Value}" :
        info.DccAddress == -1 ? "FREMO":
        Highlight(Resources.Notes.Missing) :
        "-";

    public static string LocoNumberOrMissingOrWagonNumber(this VehicleStartInfo info) =>
        info.IsVehicleWithDccAddress() && info.OwnerName.HasValue() ?
            info.VehicleNumber.HasValue() ? $"{info.VehicleNumber}" :
            Highlight(Resources.Notes.Missing) :
        info.MaxNumberOfVehicles == 1 ? info.VehicleNumber ?? "-" : Resources.Notes.NotApplicable;


    public static string BackColor(this VehicleStartInfo info, bool isPerOwner = false, bool isOverview = false) =>
        isOverview ?
            info.ReplaceOrder <= 1 ? "white" : "gainsboro" :
        isPerOwner && info.ReplaceOrder <= 1 ? "white" :
        info.ReplaceOrder <= 1 ?
            info.DayFlags == 127 ? "white" : info.IsFirstDay ? "lightyellow" : "#e6f5ff" :
        "gainsboro";
}
