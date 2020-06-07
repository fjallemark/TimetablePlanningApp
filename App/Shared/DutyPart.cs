using System.Collections.Generic;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class DutyPart
    {
        public DutyPart(Train train, TrainPart trainPart) { Train = train; TrainPart = trainPart; }
        public Train Train { get; }
        public TrainPart TrainPart { get; }
        public string Loco { get; set; } = string.Empty;
    }

    public static class DutyPartExtensions
    {
    }
}
