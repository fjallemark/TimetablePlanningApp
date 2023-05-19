using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Server.Services;

internal static class DutyBookletExtensions
{
    public static void MergeTrainPartsWithSameTrainNumber(this IEnumerable<DriverDuty> me)
    {
        foreach (var duty in me) duty.Parts = duty.Parts.MergeTrainPartsWithSameTrainNumber();
    }

    private static ICollection<DriverDutyPart> MergeTrainPartsWithSameTrainNumber(this IEnumerable<DriverDutyPart> me)
    {
        if (me.Count() == 1) return me.ToArray();
        var result = new List<DriverDutyPart>();
        var trainNumberGroups = me.GroupBy(dd => dd.Train.Number);
        foreach (var group in trainNumberGroups)
        {
            foreach(var item in group)
            {
                foreach (var call in item.Calls())
                {
                    if (call.Id == item.FromCallId || call.Id == item.ToCallId) call.AddAutomaticNotes();
                }
            }
            result.Add(new()
            {
                Train = group.First().Train,
                FromCallId = group.First().FromCallId,
                ToCallId = group.Last().ToCallId,
                Locos = group.SelectMany(p => p.Locos).ToArray()
            });
        }

        //if (me.Count() == me.Select(m => m.Train.Number).Distinct().Count()) return me.ToArray();
        //var parts = me.OrderBy(m => m.StartTime()).ToArray();
        //for (var i = 0; i < parts.Length; i++)
        //{
        //    if (i < parts.Length - 1 && parts[i].Train.Equals(parts[i + 1].Train))
        //    {
        //        var train = parts[i].Train;
        //        var part1 = parts[i];
        //        var part2 = parts[i + 1];
        //        foreach (var call in part1.Calls())
        //        {
        //            if (call.Id == part1.FromCallId || call.Id == part1.ToCallId) call.AddAutomaticNotes();
        //        }
        //        foreach (var call in part2.Calls())
        //        {
        //            if (call.Id == part2.FromCallId || call.Id == part2.ToCallId) call.AddAutomaticNotes();
        //        }
        //        result.Add(new DriverDutyPart
        //        {
        //            Train = train,
        //            FromCallId = part1.FromCallId,
        //            ToCallId = part2.ToCallId,
        //            Locos = part1.Locos.Concat(part2.Locos).ToArray(),
        //        });
        //        i++;
        //    }
        //    else
        //    {
        //        result.Add(parts[i]);
        //    }
        //}
        return result;
    }

    public static int AddTrainCallNotes(this IEnumerable<DriverDuty> me, IEnumerable<TrainCallNote> trainCallNotes)
    {
        var notes = trainCallNotes.ToDictionary();
        var count = 0;
        foreach (var duty in me)
        {
            foreach (var part in duty.Parts)
            {
                foreach (var call in part.Calls())
                {
                    call.AddAutomaticNotes();
                    var callNotes = notes.AtCall(call.Id);
                    count += callNotes.Count;
                    call.AddGeneratedNotes(duty, part, callNotes);
                }
            }
        }
        return count;
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

    private static IList<TrainCallNote> AtCall(this IDictionary<int, IList<TrainCallNote>> me, int callId) => 
        me.TryGetValue(callId, out var result) ? result : Array.Empty<TrainCallNote>();
}
