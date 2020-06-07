using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class DriverDuty
    {
        public string MeetingName { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public string Days { get; set; } = string.Empty;
        public int Difficulty { get; set; } = 1;
#pragma warning disable CS8602 // Dereference of a possibly null reference.
        public string StartTime { get; set; } = string.Empty;
        public string EndTime  { get; set; } = string.Empty;

#pragma warning restore CS8602 // Dereference of a possibly null reference.
        public IList<DutyPart> Parts { get; } = new List<DutyPart>();
    }
}
