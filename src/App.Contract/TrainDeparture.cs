using System;
using System.Collections.Generic;
using System.Text;

namespace Tellurian.Trains.Planning.App.Contract
{
    public class TrainDeparture
    {
        public string StationName { get; set; } = string.Empty;
        public string TrackNumber { get; set; } = string.Empty;
        public Loco Loco { get; set; } = new Loco();
        public TrainInfo Train { get; set; } = new TrainInfo();
        public CallTime DepartureTime { get; set; } = new CallTime();
    }
}
