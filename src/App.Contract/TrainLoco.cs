﻿using Tellurian.Trains.Planning.App.Contracts.Extensions;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts;

public abstract class Vehicle
{
    public string OperatorName { get; set; } = string.Empty;
    public string Class { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public bool IsRailcar { get; set; }
    public virtual int TurnusNumber { get; set; } = 0;
    public virtual byte OperationDaysFlags { get; set; } = OperationDays.OnDemand;

}
public class TrainLoco : Vehicle
{
    public override int TurnusNumber { get; set; }
    public override byte OperationDaysFlags { get; set; } 
    public override string ToString() => $"{OperationDaysFlags.OperationDays()} {OperatorName} {TurnusNumber}".TrimStart();
    public static TrainLoco Empty { get; } = new TrainLoco();
}

public static class LocoExtensions
{
    public static OperationDays OperationDays(this TrainLoco me) => me.OperationDaysFlags.OperationDays();
    public static string TypeName(this TrainLoco me) =>
        me.IsRailcar ? Notes.Railcar : Notes.Loco;

    public static string DisplayFormat(this TrainLoco me) => me.OperatorName.HasValue() ? 
        $"""<span style="{Style}">{me.OperatorName} {me.Class} {me.TypeName().ToLowerInvariant()} {Notes.VehicleScheduleNumber.ToLowerInvariant()} {me.TurnusNumber}</span>""" :
        $"""<span style="{Style}">{me.TypeName()} {me.Class} {Notes.VehicleScheduleNumber.ToLowerInvariant()} {me.TurnusNumber}</span>""";

    private const string Style = "font-weight: bold; background-color: white";
}
