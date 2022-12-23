using System.Collections.Generic;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Contracts.Extensions;

public static class NoteExtensions
{
    public static IEnumerable<LocoDepartureCallNote> Aggregated(this IEnumerable<LocoDepartureCallNote> callNotes)
    {
        var notes = callNotes.OrderBy(n => n.CallId).ThenBy(n => n.DepartingLoco.TurnusNumber).ToArray();
        if (notes.Length == 0) return notes;
        var result = new List<LocoDepartureCallNote>(notes.Length)
        {
            notes[0]
        };
        for (int i = 1; i < notes.Length; i++)
        {
            if (result.Last().CallId == notes[i].CallId && result.Last().DepartingLoco.TurnusNumber == notes[i].DepartingLoco.TurnusNumber)
            {
                result.Last().DepartingLoco.OperationDaysFlags |= notes[i].DepartingLoco.OperationDaysFlags;
            }
            else
            {
                result.Add(notes[i]);
            }
        }
        return result.ToList();
    }

    public static IEnumerable<LocoArrivalCallNote> Aggregated(this IEnumerable<LocoArrivalCallNote> callNotes)
    {
        var notes = callNotes.OrderBy(n => n.CallId).ThenBy(n=> n.ArrivingLoco.TurnusNumber).ThenBy(n => n.GetType().Name).ToArray();
        if (notes.Length == 0) return notes;
        var result = new List<LocoArrivalCallNote>(notes.Length)
        {
            notes[0]
        };
        for (int i = 1; i < notes.Length; i++)
        {
            if (result.Last().CallId == notes[i].CallId && result.Last().ArrivingLoco.TurnusNumber == notes[i].ArrivingLoco.TurnusNumber && result.Last().GetType() == notes[i].GetType())
            {
                result.Last().ArrivingLoco.OperationDaysFlags |= notes[i].ArrivingLoco.OperationDaysFlags;
            }
            else
            {
                result.Add(notes[i]);
            }
        }
        return result.ToList();
    }
}
