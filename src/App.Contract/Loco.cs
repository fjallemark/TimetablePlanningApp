using Tellurian.Trains.Planning.App.Contracts.Extensions;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts;

public class Loco
{
    public string OperatorName { get; set; } = string.Empty;
    public int TurnusNumber { get; set; }
    public byte OperationDaysFlags { get; set; } 
    public string Class { get; set; } = string.Empty;
    public string Number { get; set; } = string.Empty;
    public bool IsRailcar { get; set; }
    public override string ToString() => $"{OperationDaysFlags.OperationDays()} {OperatorName} {TurnusNumber}".TrimStart();
    public static Loco Empty { get; } = new Loco();
}

public static class LocoExtensions
{
    public static OperationDays OperationDays(this Loco me) => me.OperationDaysFlags.OperationDays();
    public static string TypeName(this Loco me) =>
        me.IsRailcar ? Notes.Railcar : Notes.Loco;

    public static string DisplayFormat(this Loco me) => me.OperatorName.HasValue() ? 
        $"[{me.OperatorName} {me.TypeName().ToLowerInvariant()} {Notes.VehicleScheduleNumber.ToLowerInvariant()} {me.TurnusNumber}]" :
        $"[{me.TypeName()} {Notes.VehicleScheduleNumber.ToLowerInvariant()} {me.TurnusNumber}]";
}
