using System.Collections.Generic;
using Tellurian.Trains.Planning.App.Contracts.Resources;

#pragma warning disable CA2227 // Collection properties should be read only

namespace Tellurian.Trains.Planning.App.Contracts
{
    public abstract class VehicleSchedule
    {
        public abstract string Type { get; }
        public virtual bool IsLoco => false;
        public virtual bool IsTrainset => false;
        public string Number { get; set; } = string.Empty;
        public OperationDays OperationDays { get; set; } = new OperationDays();
        public string Operator { get; set; } = string.Empty;
        public string Note { get; set; } = string.Empty;
        public bool TurnForNextDay { get; set; }
        public string Class { get; set; } = string.Empty;
        public IList<TrainPart> TrainParts { get; set; } = new List<TrainPart>();
    }

    public class LocoSchedule : VehicleSchedule
    {
        public override string Type => "Loco";
        public override bool IsLoco => true;
    }

    public class TrainsetSchedule : VehicleSchedule
    {
        public override string Type => "Trainset";
        public override bool IsTrainset => true;
    }

    public static class VehicleScheduleExtensions
    {
        public static IEnumerable<(string label, string? value)> HeaderItems(this VehicleSchedule? me)
        {
            var result = new List<(string label, string? value)>();
            if (me is null) return result;
            result.Add((me.Type, me.Number));
            if (!string.IsNullOrWhiteSpace(me.OperationDays.ShortName)) result.Add((Notes.Days, me.OperationDays.ShortName));
            if (!string.IsNullOrWhiteSpace(me.Operator)) result.Add((Notes.Operator, me.Operator));
            return result;
        }
    }
}

