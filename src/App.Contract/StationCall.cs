using System.Collections.Generic;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Contract
{
    public class StationCall
    {
        public int Id { get; set; }
        public Station Station { get; set; } = new Station();
        public string Track { get; set; } = string.Empty;
        public bool IsStop { get; set; } = true;
        public CallTime? Arrival { get; set; }
        public CallTime? Departure { get; set; }
        public int SequenceNumber { get; set; }
        public override string ToString() => $"{Id} {Station.Signature} {Track} Arr:{Arrival} Dep:{Departure}";
        public override bool Equals(object obj) => obj is StationCall other && other.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }

    public static class StationCallExtensions
    {
        public static CallAction AsArrival(this StationCall call) =>
            new CallAction
            {
                Station = call.Station,
                Track = call.Track,
                Time = call.Arrival,
            };

        public static CallAction AsDeparture(this StationCall call) =>
            new CallAction
            {
                Station = call.Station,
                Track = call.Track,
                Time = call.Departure,
            };

        public static bool AddArrivalNote(this StationCall me, Note note)
        {
            if (me.Arrival is null) return false;
            if (me.Arrival.ContainsSimilarNote(note)) return false;
            if (me.IsStop) me.Arrival.Notes.Add(note);
            else if(me.Departure is not null) me.Departure.Notes.Add(note);
            return true;
        }

        public static bool AddDepartureNote(this StationCall me, Note note)
        {
            if (me.Departure is null) return false;
            if (me.Departure.ContainsSimilarNote(note)) return false;
            me.Departure.Notes.Add(note);
            return true;
        }

        public static void AddAutomaticNotes(this DutyStationCall me)
        {
            if (!me.IsStop && me.IsDepartureInDuty) me.AddDepartureNote(new Note { DisplayOrder = -30001, Text = Resources.Notes.NoStop });
        }

        public static void AddGeneratedNotes(this DutyStationCall me, DutyPart part, IEnumerable<TrainCallNote> notes)
        {
            var train = part.Train;
            foreach (var note in notes)
            {
                note.TrainInfo = train;
                if (note.IsForArrival && me.IsArrivalInDuty) foreach (var n in note.ToNotes()) me.AddArrivalNote(n);
                if (note.IsForDeparture && me.IsDepartureInDuty) foreach (var n in note.ToNotes()) me.AddDepartureNote(n);
            }
        }

        private static bool ContainsSimilarNote(this CallTime? me, Note note)
        {
            if (me is null) return false;
            return me.Notes.Any(n => n.Text.StartsWith(note.Text[0..^1], System.StringComparison.OrdinalIgnoreCase));
        }
    }
}
