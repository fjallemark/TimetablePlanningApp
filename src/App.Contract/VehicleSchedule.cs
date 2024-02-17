using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts;

public abstract class VehicleSchedule
{
    public required string Type { get; set; }
    public bool IsLoco { init; get; }
    public bool IsTrainset { init; get; }
    public bool IsCargoOnly { init; get; }
    public string TurnusNumber { get; set; } = string.Empty;
    public string VehicleNumber { get; set; } = string.Empty;
    public OperationDays OperationDays { get; set; } = new OperationDays();
    public string Operator { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public bool TurnForNextDay { get; set; }
    public string Class { get; set; } = string.Empty;
    public int NumberOfUnits { get; set; } = 1;
    public int ReplaceOrder { get; set; }
    public bool PrintCard { get; set; }
    public IList<TrainPart> TrainParts { get; set; } = new List<TrainPart>();
}

public class LocoSchedule : VehicleSchedule
{
    public LocoSchedule()
    {
        IsLoco = true;
    }

    public bool IsRailcar { get; set; }
}

public class TrainsetSchedule : VehicleSchedule
{
    public TrainsetSchedule()
    {
        IsTrainset = true;
    }
}

public class CargoOnlySchedule : VehicleSchedule
{
    public CargoOnlySchedule()
    {
        IsCargoOnly = true;
    }
}

public static class VehicleScheduleExtensions
{
    public static IEnumerable<(string label, string? value)> HeaderItems(this VehicleSchedule? me)
    {
        var result = new List<(string label, string? value)>();
        if (me is null) return result;
        result.Add((me.Type, me.TurnusNumber));
        if (!string.IsNullOrWhiteSpace(me.OperationDays.ShortName)) result.Add((Notes.Days, me.OperationDays.ShortName));
        if (!string.IsNullOrWhiteSpace(me.Operator)) result.Add((Notes.Operator, me.Operator));
        return result;
    }

    public static IEnumerable<TSchedule> MergePartsOfSameTrain<TSchedule>(this IEnumerable<TSchedule> schedules) where TSchedule : VehicleSchedule
    {
        var result = new List<TSchedule>(schedules.Count());
        foreach (var schedule in schedules)
        {
            var trains = schedule.TrainParts
                .Where(tp => tp.Train is not null && tp.Train.OperationDaysFlags.IsAnyOtherDays(schedule.OperationDays.Flags))
                .GroupBy(tp => $"{tp.Train?.OperatorName}{tp.TrainNumber}");
            if (trains.Count() < schedule.TrainParts.Count)
            {
                var newParts = new List<TrainPart>(trains.Count());
                foreach (var part in trains)
                {
                    var newPart = new TrainPart()
                    {

                        FromDeparture = part.First().FromDeparture,
                        ToArrival = part.Last().ToArrival,
                        LocoNumber = part.First().LocoNumber,
                        Train = part.First().Train,
                        TrainNumber = part.First().TrainNumber
                    };
                    newParts.Add(newPart);
                }
                schedule.TrainParts = newParts.ToArray();
            }

            result.Add(schedule);
        }
        return result;
    }

    public static string TurnusTypeName(this VehicleSchedule me) =>
        me is LocoSchedule loco ?
        loco.IsRailcar ? Notes.Railcar : Notes.Loco :
        me.Type == "CargoOnly" ? Notes.CargoTurnus :
        me.PrintCard && me.NumberOfUnits > 1 ? Notes.Wagonset : Notes.WagonTurnus;

    public static string? Note(this VehicleSchedule me) =>
        me.HasIndividualWagonCards() ? me.Note :
        me.NumberOfUnits > 1 ? $"{me.NumberOfUnits}×{me.Note}" : me.Note;

    public static string CrossLineColor(this VehicleSchedule me) =>
        me.IsLoco ? "#ffc0cb" :
        me.Type == "CargoOnly" ? "#ffff99" :
        me.Type == "PassengerWagon" ? "#66ff99" :
        me.Type == "CargoWagon" ? "#b3d9ff" :
        "#cccccc";

    public static bool HasIndividualWagonCards(this VehicleSchedule schedule) =>
        !schedule.PrintCard && schedule.NumberOfUnits > 1;

    public static IEnumerable<VehicleSchedule> SchedulesToPrint(this IEnumerable<VehicleSchedule>? schedules)
    {
        if (schedules is null) return Enumerable.Empty<VehicleSchedule>();
        var x = schedules.GroupBy(s => s.TurnusNumber);
        var result = new List<VehicleSchedule>(200);
        foreach(var g in x)
        {
            var numberToPrint = g.First().HasIndividualWagonCards() ? g.First().NumberOfUnits : 1;
            for (var i = 0; i < numberToPrint; i++)
            {
                foreach(var v in g) result.Add(v);
            }
 

            
        }
        //foreach (var schedule in schedules ?? Enumerable.Empty<TrainsetSchedule>())
        //{
        //    if (schedule.HasIndividualWagonCards()) { for (int i = 0; i < schedule.NumberOfUnits; i++) { yield return schedule; } }
        //    else yield return schedule;
        //}
        return result;
    }
}

