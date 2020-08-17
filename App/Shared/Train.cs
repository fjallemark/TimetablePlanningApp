using System;
using System.Collections.Generic;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class TrainInfo
    {
        public string Number { get; set; } = string.Empty;
        public string OperatorName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;
        public OperationDays OperationDays { get; set; } = new OperationDays();
        public override string ToString() => $"{OperatorName} {Number} {OperationDays.ShortName}";

    }

    public class Train : TrainInfo
    {
        public int MaxSpeed { get; set; }
        public int MaxNumberOfWaggons { get; set; }
        public string Instruction { get; set; } = string.Empty;
        public IList<StationCall> Calls { get; set; } = Array.Empty<StationCall>();
        public override string ToString() => $"{base.ToString()} {Calls.Count} calls";
    }
}
