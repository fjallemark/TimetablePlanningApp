using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Tellurian.Trains.Planning.App.Contracts;
public class StationCallWithAction
{
    /// <summary>
    /// This type should only contain trains that stops and where there is at least one note.
    /// </summary>
    /// <param name="station"></param>
    /// <param name="trackNumber"></param>
    /// <param name="callTime"></param>
    public StationCallWithAction(TrainInfo train, StationCall call, bool isArrival = false, bool isShuntingOnly = false)
    {
        Train = train;
        Call = call;
        IsArrival = isArrival;
        IsShuntingOnly = isShuntingOnly;
    }
    [JsonConstructor]
    public StationCallWithAction(TrainInfo train, StationCall call, IEnumerable<Note> notes, bool isArrival = false, bool isShuntingOnly = false)
    {
        Train = train;
        Call = call;
        IsArrival = isArrival;
        IsShuntingOnly = isShuntingOnly;
        Notes = notes;

    }
    public StationCall Call { get; init; }
    public TrainInfo Train { get; init; }
    public bool IsArrival { get; init; }
    public bool IsShuntingOnly { get; init; }
    public IEnumerable<Note> Notes { get; private set; } = Enumerable.Empty<Note>();
    [JsonIgnore] public bool IsDeparture => !IsArrival;
    [JsonIgnore] private bool UseShuntingNotes => Call.Station.HasCombinedInstructions || IsShuntingOnly;
    [JsonIgnore] private bool UseStationNotes => Call.Station.HasCombinedInstructions || !IsShuntingOnly;
    [JsonIgnore] public StationInfo Station => Call.Station;
    [JsonIgnore] public string ArrivalTime => IsArrival ? Call.Arrival!.IsHidden ? "" : Call.Arrival!.Time : $"({Call.Arrival?.Time})";
    [JsonIgnore] public string DepartureTime => IsDeparture ? Call.Departure!.IsHidden ? "" : Call.Departure!.Time : $"({Call.Departure?.Time})";
    [JsonIgnore] public string SortTime => IsArrival ? ArrivalTime : DepartureTime;
    [JsonIgnore] public int Rows => Notes.Any() ? 1 + Notes.Count() : 0;

    public void AddNotes(IEnumerable<TrainCallNote> notes) => 
        Notes = notes.Where(n => AddNote(n))
        .SelectMany(n => n.ToNotes())
        .Distinct()
        .OrderBy(n => n.DisplayOrder)
        .ThenBy(n => n.Text)
        .ToList();

    private bool AddNote(TrainCallNote n) =>
        (n.CallId == Call.Id) &&
        (n.IsForArrival == IsArrival || n.IsForDeparture == IsDeparture) &&
        (n.IsStationNote == UseStationNotes || n.IsShuntingNote == UseShuntingNotes);

}
