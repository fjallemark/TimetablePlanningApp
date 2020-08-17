using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class DriverDuty
    {
        public string Operator { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Name { get; set; } = string.Empty;
        public OperationDays OperationDays { get; set; } = new OperationDays();
        public int Difficulty { get; set; } = 1;
        public string StartTime { get; set; } = string.Empty;
        public string EndTime  { get; set; } = string.Empty;
        public int RemoveOrder { get; set; }
        public ICollection<DutyPart> Parts { get; set; } = new List<DutyPart>();

    }

    public static class DriverDutyExtensions
    {
        public static string? StartTime(this DriverDuty me) =>me.StartTime.HasValue() ? me.StartTime : me.Parts.OrderBy(p => p.StartTime()).First().StartTime();
        public static string? EndTime(this DriverDuty me) => me.EndTime.HasValue() ? me.EndTime : me.Parts.OrderBy(p => p.StartTime()).Last().EndTime();
        public static string TrainOperatingDay(this DriverDuty me, Train train) => me is null ? "" : me.OperationDays.Equals(train.OperationDays) ? "" : train.OperationDays.FullName;
    }
}
