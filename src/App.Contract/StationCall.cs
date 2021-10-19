using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public class StationCall
    {
        public int Id { get; set; }
        public StationInfo Station { get; set; } = new StationInfo();
        public int TrackId { get; set; }
        public string TrackNumber { get; set; } = string.Empty;
        public bool IsStop { get; set; } = true;
        public CallTime? Arrival { get; set; }
        public CallTime? Departure { get; set; }
        public int SequenceNumber { get; set; }
        public override string ToString() => $"{Id} {Station.Signature} {TrackNumber} Arr:{Arrival} Dep:{Departure}";
        public override bool Equals(object? obj) => obj is StationCall other && other.Id == Id;
        public override int GetHashCode() => Id.GetHashCode();
    }

    public static class StationCallExtensions
    {
        public static CallAction AsArrival(this StationCall call) =>
            new()
            {
                Station = call.Station,
                Track = call.TrackNumber,
                Time = call.Arrival,
            };

        public static CallAction AsDeparture(this StationCall call) =>
            new()
            {
                Station = call.Station,
                Track = call.TrackNumber,
                Time = call.Departure,
            };

        public static bool IsArrival([NotNullWhen(true)] this StationCall call) =>
            call.IsStop && call.Arrival.HasValue();

        public static bool IsDeparture([NotNullWhen(true)] this StationCall call) =>
            call.IsStop && call.Departure.HasValue();

        private static bool HasValue([NotNullWhen(true)] this CallTime? me) => me is not null;

        public static CallTime[] CallTimes(this StationCall me)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            var result = new List<CallTime>();
            if (me.IsArrival()) result.Add(me.Arrival);
            if (me.IsDeparture()) result.Add(me.Departure);
            return result.ToArray();
#pragma warning restore CS8604 // Possible null reference argument.
        }

        public static void AddAutomaticNotes(this DutyStationCall me)
        {
            if (!me.IsStop && me.IsDepartureInDuty) me.AddDepartureNote(new Note { DisplayOrder = -30001, Text = Resources.Notes.NoStop });
        }

        public static double SortTime(this CallTime me) =>
            me.OffsetMinutes();

        public static void AddGeneratedNotes(this DutyStationCall me, DriverDuty duty, DutyPart part, IEnumerable<TrainCallNote> notes)
        {
            var train = part.Train;
            foreach (var note in notes)
            {
                note.TrainInfo = train;
                if (note.IsForArrival && me.IsArrivalInDuty) foreach (var n in note.ToNotes(duty.OperationDays.Flags)) me.AddArrivalNote(n);
                if (note.IsForDeparture && me.IsDepartureInDuty) foreach (var n in note.ToNotes(duty.OperationDays.Flags)) me.AddDepartureNote(n);
            }
        }

        public static void AddGereratedNotes(this IEnumerable<Train> trains, IEnumerable<TrainCallNote> notes)
        {
            var calls = trains.SelectMany(t => t.Calls).ToDictionary(c => c.Id);
            foreach (var train in trains)
            {
                foreach (var note in notes)
                {
                    var call = calls[note.CallId];
                    note.TrainInfo = train;
                    if (note.IsForArrival && call.IsArrival()) foreach (var n in note.ToNotes()) call.AddArrivalNote(n);
                    if (note.IsForDeparture && call.IsDeparture()) foreach (var n in note.ToNotes()) call.AddDepartureNote(n);
                }
            }
        }

        public static bool AddArrivalNote(this StationCall me, Note note)
        {
            if (me.Arrival is null) return false;
            if (me.Arrival.ContainsSimilarNote(note)) return false;
            var similarNote = me.Arrival.Notes.FirstOrDefault(n => note.Text.StartsWith(n.Text[0..^1], StringComparison.OrdinalIgnoreCase));
            if (similarNote is not null)
            {
                similarNote.Text = note.Text;
                return true;
            }
            if (me.IsStop) me.Arrival.Notes.Add(note);
            else if (me.Departure is not null) me.Departure.Notes.Add(note);
            return true;
        }

        public static bool AddDepartureNote(this StationCall me, Note note)
        {
            if (me.Departure is null) return false;
            if (me.Departure.ContainsSimilarNote(note)) return false;
            var similarNote = me.Departure.Notes.FirstOrDefault(n => note.Text.StartsWith(n.Text[0..^1], StringComparison.OrdinalIgnoreCase));
            if (similarNote is not null)
            {
                similarNote.Text = note.Text;
                return true;
            }
            me.Departure.Notes.Add(note);
            return true;
        }
        private static bool ContainsSimilarNote(this CallTime? me, Note note)
        {
            if (me is null) return false;
            return me.Notes.Any(n => n.Text.StartsWith(note.Text[0..^1], StringComparison.OrdinalIgnoreCase));
        }
    }
}
