﻿using System.Diagnostics;
using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Server.Extensions;

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
            var sortedGroup = group.OrderBy(g => g.StartTime()).ToList();
            foreach (var item in sortedGroup)
            {
                foreach (var call in item.Calls())
                {
                    if (call.Id == item.FromCallId || call.Id == item.ToCallId) call.AddAutomaticNotes();
                }
            }
            result.Add(new()
            {
                Train = sortedGroup.First().Train,
                FromCallId = sortedGroup.First().FromCallId,
                ToCallId = sortedGroup.Last().ToCallId,
                Locos = sortedGroup.SelectMany(p => p.Locos).ToArray(),
                IsReinforcement = sortedGroup.First().IsReinforcement,
                
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

    public static int AddTrainCallNotes(this IEnumerable<DriverDuty> me, IEnumerable<TrainCallNote> trainCallNotes, bool useCompactNotes = true)
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
                    if (notes.TryGetValue(call.Id, out var callNotes))
                    {
                        count += callNotes.Count;
                        call.AddGeneratedNotes(duty, part, callNotes, useCompactNotes);
                    }
                }
            }
        }
        return count;
    }
}
