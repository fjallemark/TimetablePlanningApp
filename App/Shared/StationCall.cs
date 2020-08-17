using System.Collections.Generic;

namespace Tellurian.Trains.Planning.App.Shared
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
            me.Arrival.Notes.Add(note);
            return true;
        }

        public static bool AddDepartureNote(this StationCall me, Note note)
        {
            if (me.Departure is null) return false;
            me.Departure.Notes.Add(note);
            return true;
        }

        public static void AddAutomaticNotes(this StationCall me, DutyPart part)
        {
            if (!me.IsStop) me.AddDepartureNote(new Note { DisplayOrder = -30001, Text = Resources.Notes.NoStop });
            if (part.IsFirstDutyCall(me))
            {
                if (part.GetLocoAtParking) me.AddDepartureNote(new Note { DisplayOrder = -30003, Text = Resources.Notes.GetLocoAtParking });
            }
            if (part.IsLastDutyCall(me))
            {
                if (part.TurnLoco) me.AddArrivalNote(new Note { DisplayOrder = 30001, Text = Resources.Notes.TurnLoco });
                if (part.ReverseLoco) me.AddArrivalNote(new Note { DisplayOrder = 30002, Text = Resources.Notes.ReverseLoco });
                if (part.PutLocoAtParking) me.AddArrivalNote(new Note { DisplayOrder = 30003, Text = Resources.Notes.PutLocoAtPArking });
            }
        }

        public static void AddGeneratedNotes(this StationCall me, Train train, IEnumerable<TrainCallNote> notes)
        {
            foreach (var note in notes)
            {
                note.TrainInfo = train;
                if (note.IsForArrival) foreach (var n in note.ToNotes()) me.AddArrivalNote(n);
                if (note.IsForDeparture) foreach (var n in note.ToNotes()) me.AddDepartureNote(n);
            }

        }
    }
}
