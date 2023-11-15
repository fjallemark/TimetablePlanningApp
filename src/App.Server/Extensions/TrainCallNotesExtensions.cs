using Tellurian.Trains.Planning.App.Contracts;

namespace Tellurian.Trains.Planning.App.Server.Extensions;

public static class TrainCallNotesExtensions
{
    public static Dictionary<int, List<TrainCallNote>> ToDictionary(this IEnumerable<TrainCallNote> callNotes)
    {
        var result = new Dictionary<int, List<TrainCallNote>>();
        foreach (var callNote in callNotes)
        {
            var key = callNote.CallId;
            if (!result.ContainsKey(key)) result.Add(key, []);
            result[key].Add(callNote);
        }
        return result;
    }
}
