using System.Collections.Generic;

#pragma warning disable CA2227 // Collection properties should be read only

namespace Tellurian.Trains.Planning.App.Shared
{
    public abstract class VehicleSchedule
    {
        public abstract string Type { get; }
        public string Number { get; set; } = string.Empty;
        public string? Days { get; set; }
        public string? Operator { get; set; }
        public string? Note { get; set; }
        public bool TurnForNextDay { get; set; }
        public string? Class { get; set; }
        public IList<TrainPart> TrainParts { get; set; } = new List<TrainPart>();
    }

    public class LocoSchedule : VehicleSchedule
    {
        public override string Type => "Loco";
    }

    public class TrainsetSchedule : VehicleSchedule
    {
        public override string Type => "Trainset";
    }

    public static class VehicleScheduleExtensions
    {
        public static IEnumerable<(string label, string? value)> HeaderItems(this VehicleSchedule? me)
        {
            var result = new List<(string label, string? value)>();
            if (me is null) return result;
            result.Add((me.Type, me.Number));
            if (!string.IsNullOrWhiteSpace(me.Days)) result.Add(("Days", me.Days));
            if (!string.IsNullOrWhiteSpace(me.Operator)) result.Add(("Operator", me.Operator));
            return result;
        }
    }
}

