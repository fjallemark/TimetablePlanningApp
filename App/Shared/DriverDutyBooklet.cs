using System;
using System.Collections.Generic;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class DriverDutyBooklet
    {
        public string ScheduleName { get; set; } = string.Empty;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "CA2227:Collection properties should be read only", Justification = "Required for deserialization.")]
        public ICollection<DriverDuty> Duties { get; set; } = Array.Empty<DriverDuty>();

        public static DriverDutyBooklet Example => new DriverDutyBooklet
        {
            ScheduleName = "Demo",
            Duties = new[]
            {
                new DriverDuty
                {
                    Operator = "Green Cargo",
                         OperationDays = ((byte)31).OperationDays(),
                         Difficulty = 2,
                         Name = "Chemicals transport",
                         Number=22,
                         StartTime = "11:40",
                         EndTime = "15:38",
                         Parts = new [] {
                             new DutyPart(Train.Example, new Loco {  OperatorName="GC", Number=52}, 22, 27)
                         }
                }
            }
        };
    }

    public static class DriverDutyBookletExtensions
    {
        public static void MergeTrainPartsWithSameTrainNumber(this DriverDutyBooklet me)
        {
            foreach(var duty in me.Duties)
            {
                duty.Parts = duty.Parts.MergeTrainPartsWithSameTrainNumber();
            }
        }

        internal static ICollection<DutyPart> MergeTrainPartsWithSameTrainNumber(this IEnumerable<DutyPart> me)
        {
            if (me.Count() == 1) return me.ToArray();
            if (me.Count() == me.Select(m => m.Train.Number).Distinct().Count()) return me.ToArray();
            var parts = me.OrderBy(m => m.StartTime()).ToArray();
            var result = new List<DutyPart>();
            for (var i =0; i < parts.Length-1; i++)
            {
                if (parts[i].Train.Number == parts[i+1].Train.Number)
                {
                    var train = parts[i].Train;
                    var part1 = parts[i];
                    var part2 = parts[i + 1];
                    foreach (var call in part1.Calls())
                    {
                        if (call.Id == part1.FromCallId || call.Id == part1.ToCallId) call.AddAutomaticNotes();
                    }
                    foreach( var call in part2.Calls())
                    {
                        if (call.Id == part2.FromCallId || call.Id == part2.ToCallId) call.AddAutomaticNotes();
                    }
                    result.Add(new DutyPart
                    {
                        Train = train,
                        FromCallId = part1.FromCallId,
                        ToCallId = part2.ToCallId,
                        Locos = part1.Locos.Concat(part2.Locos).ToArray(),
                    });
                }
                else
                {
                    result.Add(parts[i]);
                }
            }
            return result;
        }
        public static void AddTrainCallNotes(this DriverDutyBooklet me, IEnumerable<TrainCallNote> trainCallNotes)
        {
            var notes = trainCallNotes.ToDictionary();
            foreach (var duty in me.Duties)
            {
                foreach (var part in duty.Parts)
                {
                    foreach (var call in part.Calls())
                    {
                        call.AddAutomaticNotes();
                        call.AddGeneratedNotes(part, notes.Item(call.Id));
                    }
                }
            }
        }

        private static IDictionary<int, IList<TrainCallNote>> ToDictionary(this IEnumerable<TrainCallNote> me)
        {
            var result = new Dictionary<int, IList<TrainCallNote>>(1000);
            foreach (var n in me)
            {
                var key = n.CallId;
                if (!result.ContainsKey(key)) result.Add(key, new List<TrainCallNote>());
                result[key].Add(n);
            }
            return result;
        }

        private static IList<TrainCallNote> Item(this IDictionary<int, IList<TrainCallNote>> me, int callId) =>
            me.ContainsKey(callId) ? me[callId] : Array.Empty<TrainCallNote>();
    }
}
