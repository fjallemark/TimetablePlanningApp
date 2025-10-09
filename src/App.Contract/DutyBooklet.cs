using System.Globalization;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts;

public abstract class DutyBooklet
{
    public string ScheduleName { get; set; } = string.Empty;
    public DateTime ValidFromDate { get; set; }
    public DateTime ValidToDate { get; set; }
    public IEnumerable<Instruction> Instructions { get; set; } = [];

}

public static class DutyBookletExtensions
{
    public static string ValidPeriod(this DutyBooklet me) =>
        string.Format(CultureInfo.CurrentCulture, Notes.ValidPeriod, me.ValidFromDate, me.ValidToDate);

}

public sealed class DriverDutyBooklet : DutyBooklet
{
    public ICollection<DriverDuty> Duties { get; set; } = Array.Empty<DriverDuty>();

    public static DriverDutyBooklet Example => new()
    {
        ScheduleName = "Demo",
        Duties =
        [
            new DriverDuty
            {
                Operator = "Green Cargo",
                OperationDays = ((byte)31).OperationDays(),
                Difficulty = 2,
                Description = "Chemicals transport",
                Number="22",
                StartTime = "11:40",
                EndTime = "15:38",
                Parts = [
                         new DriverDutyPart(Train.Example, new TrainLoco {  OperatorName="GC", TurnusNumber=52}, 22, 27)
                     ]
            }
        ]
    };
}

public class StationDutyBooklet : DutyBooklet
{
    public ICollection<StationDuty> Duties { get; set; } = Array.Empty<StationDuty>();
}
