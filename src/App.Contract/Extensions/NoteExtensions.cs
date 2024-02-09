namespace Tellurian.Trains.Planning.App.Contracts.Extensions;

public static class NoteExtensions
{
    /// <summary>
    /// Generic method for aggregating <see cref="TrainCallNote">train call notes</see> at same call id.
    /// </summary>
    /// <typeparam name="TNote"></typeparam>
    /// <param name="callNotes"></param>
    /// <param name="otherwiseAreSame"></param>
    /// <param name="additionalSortKey"></param>
    /// <param name="apply"></param>
    /// <returns></returns>
    internal static IEnumerable<TNote> Aggregated<TNote>(this IEnumerable<TNote> callNotes, Func<TNote, TNote, bool> otherwiseAreSame, Func<TNote, string> additionalSortKey, Action<TNote, TNote> apply) where TNote : TrainCallNote
    {
        var notes = callNotes.OrderBy(n => n.CallId).ThenBy(n => additionalSortKey(n)).ToArray();
        if (notes.Length == 0) return notes;
        var result = new List<TNote>(notes.Length)
        {
            notes[0]
        };
        for (int i = 1; i < notes.Length; i++)
        {
            if (result.Last().CallId == notes[i].CallId && otherwiseAreSame(result.Last(), notes[i]))
            {
                apply(result.Last(), notes[i]);
            }
            else
            {
                result.Add(notes[i]);
            }
        }
        return result;
    }
    public static IEnumerable<LocoDepartureCallNote> AggregateOperationDays(this IEnumerable<LocoDepartureCallNote> callNotes)
    {
        return callNotes.Aggregated(OtherwiseAreSame, AdditionalSortKey, ConcatOperationDays);

        static bool OtherwiseAreSame(LocoDepartureCallNote note1, LocoDepartureCallNote note2) =>
            note1.DepartingLoco.TurnusNumber == note2.DepartingLoco.TurnusNumber && note1.GetType() == note2.GetType();

        static string AdditionalSortKey(LocoDepartureCallNote note) =>
            $"{note.DepartingLoco.TurnusNumber:00000}-{note.GetType().Name}";

        static void ConcatOperationDays(LocoDepartureCallNote note1, LocoDepartureCallNote note2) =>
            note1.DepartingLoco.OperationDaysFlags |= note2.DepartingLoco.OperationDaysFlags;
    }

    public static IEnumerable<LocoArrivalCallNote> AggregateOperationDays(this IEnumerable<LocoArrivalCallNote> callNotes)
    {

        return callNotes.Aggregated(OtherwiseAreSame, AdditionalSortKey, ConcatOperationDays);

        static bool OtherwiseAreSame(LocoArrivalCallNote note1, LocoArrivalCallNote note2) =>
            note1.GetType() == note2.GetType() &&
            (note1.ArrivingLoco.TurnusNumber == note2.ArrivingLoco.TurnusNumber || note1.GetType() == typeof(LocoCirculationNote) || note1.GetType() == typeof(LocoTurnNote));

        static string AdditionalSortKey(LocoArrivalCallNote note) =>
            $"{note.ArrivingLoco.TurnusNumber:00000}-{note.GetType().Name}";

        static void ConcatOperationDays(LocoArrivalCallNote note1, LocoArrivalCallNote note2) =>
            note1.ArrivingLoco.OperationDaysFlags |= note2.ArrivingLoco.OperationDaysFlags;
    }

    


}
