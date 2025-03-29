using System.Globalization;
using Tellurian.Trains.Planning.App.Contracts.Extensions;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts;

public abstract class Duty
{
    public string LayoutName { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public int DisplayOrder { get; init; }
    public int Difficulty { get; init; } = 1;
    public DateTime ValidFromDate { get; init; }
    public DateTime ValidToDate { get; init; }
    public string StartTime { get; init; } = string.Empty;
    public string EndTime { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int StaffingNumber { get; init; } = 1;
}

public static class DutyExtensions
{
    public static string ValidPeriod(this Duty me) =>
        string.Format(CultureInfo.CurrentCulture, Notes.ValidPeriod, me.ValidFromDate, me.ValidToDate);

    public static string? StartTime(this Duty me) =>
        me.StartTime.HasValue() ? me.StartTime :
        me is DriverDuty driverDuty ? driverDuty.Parts.OrderBy(p => p.StartTime()).First().StartTime() :
        "##:##";

    public static string? EndTime(this Duty me) =>
       me.EndTime.HasValue() ? me.EndTime :
       me is DriverDuty driverDuty ? driverDuty.Parts.OrderBy(p => p.StartTime()).Last().EndTime() :
       "##:##";

    public static string? StartStation(this DriverDuty me) =>
        me.Parts.OrderBy(p => p.StartTime()).FirstOrDefault()?.Calls().First().Station.Name;
    public static string? EndStation(this DriverDuty me) =>
    me.Parts.OrderBy(p => p.StartTime()).LastOrDefault()?.Calls().Last().Station.Name;

    public static string Description(this DriverDuty me) => $"{Notes.Duty} {me.Number} - {me.OperationDays.ShortName}"; 
}

public class DriverDuty : Duty
{

    public string Operator { get; init; } = string.Empty;
    public OperationDays OperationDays { get; init; } = new OperationDays();
    public int RemoveOrder { get; init; }
    public ICollection<DriverDutyPart> Parts { get; set; } = [];
}

public static class DriverDutyExtensions
{

    public static string TrainOperatingDay(this DriverDuty me, Train train) =>
        me is null ? "" :
        me.OperationDays.Flags.IsAllOtherDays(train.OperationDaysFlags) ? "" :
        train.OperationDaysFlags.OperationDays().ShortName;

    public static string TrainTypes(this DriverDuty me) => string.Join(", ", me.Parts.Select(p => p.Train.CategoryName).Distinct());
}

public class StationDuty : Duty
{
    public int StationId { get; init; }
    public required string StationName { get; init; }
    public StationDutyType StationDutyType { get; init; }
    public ICollection<Instruction>? StationInstructions { get; init; }
    public ICollection<Instruction>? ShuntingInstructions { get; init; }
    public ICollection<StationCallWithAction> Calls { get; set; } = Array.Empty<StationCallWithAction>();
}

public static class StationDutyExtensions
{

}

public enum StationDutyType
{
    Single,
    TrainClearance,
    Shunting
}
