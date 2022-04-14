using System.Collections.Generic;
using System.Linq;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public abstract class VehicleSchedule
    {
        public abstract string Type { get; }
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
        public IList<TrainPart> TrainParts { get; set; } = new List<TrainPart>();
    }

    public class LocoSchedule : VehicleSchedule
    {
        public LocoSchedule() => IsLoco = true;
        public override string Type => "Loco";
        public bool IsRailcar { get; set; }
    }

    public class TrainsetSchedule : VehicleSchedule
    {
        public TrainsetSchedule() => IsTrainset = true;
        public override string Type => "Trainset";
    }

    public class CargoOnlySchedule : VehicleSchedule
    {
        public CargoOnlySchedule() => IsCargoOnly = true;
        public override string Type => "CargoOnly";
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
                    .GroupBy(tp => tp.TrainNumber);
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
            loco.IsRailcar ? Notes.RailcarTurnus : Notes.LocoTurnus :
            me.IsCargoOnly ? Notes.CargoTurnus :
            Notes.TrainsetTurnus;

        public static string? Note(this VehicleSchedule me) =>
            me.NumberOfUnits > 1 ? $"{me.NumberOfUnits}×{me.Note}" : me.Note;

        public static string CrossLineColor(this VehicleSchedule me) =>
            me.IsLoco ? "#ffc0cb" :
            me.IsCargoOnly ? "#ffff99" :
            me.IsTrainset ? "#66ff99" :
            "#cccccc";

    }
}

