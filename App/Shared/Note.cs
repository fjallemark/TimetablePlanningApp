using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Tellurian.Trains.Planning.App.Shared.Resources;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class Note
    {
        public string? Text { get; set; }
        public int DisplayOrder { get; set; }
        public override string ToString() => Text is null ? "" : Text;
    }

    public abstract class TrainCallNote
    {
        protected TrainCallNote(int callId)
        {
            CallId = callId;
        }
        public int CallId { get; }
        public int DisplayOrder { get; set; }
        public bool IsDriverNote { get; set; }
        public bool IsStationNote { get; set; }
        public bool IsShuntingNote { get; set; }
        public bool IsForArrival { get; set; }
        public bool IsForDeparture { get; set; }
        public TrainInfo? TrainInfo { get; set; }
        public abstract IEnumerable<Note> ToNotes();
    }

    public sealed class ManualTrainCallNote : TrainCallNote
    {
        // TODO: Implemnent support for multilanguage manual notes.
        public ManualTrainCallNote(int callId) : base(callId) { }
        public string Text { get; set; } = string.Empty;
        public override IEnumerable<Note> ToNotes() => new[] { new Note { DisplayOrder = DisplayOrder, Text = Text } };
    }

    public abstract class TrainsetsCallNote : TrainCallNote
    {
        protected TrainsetsCallNote(int callId) : base(callId)
        {
        }
        protected IList<Trainset> Trainsets { get; } = new List<Trainset>();
        public void AddTrainset(Trainset trainset)
        {
            Trainsets.Add(trainset);
            UpdateVisibility();
        }
        protected virtual void UpdateVisibility()
        {
            IsStationNote = Trainsets.Any(t => !t.IsCargo);
            IsShuntingNote = Trainsets.Any(t => t.IsCargo);
        }
    }

    public class TrainsetsDepartureCallNote : TrainsetsCallNote
    {
        public TrainsetsDepartureCallNote(int callId) : base(callId)
        {
            IsForDeparture = true;
        }
        protected override void UpdateVisibility()
        {
            base.UpdateVisibility();
            IsDriverNote = Trainsets.Any(t => t.HasCoupleNote);
        }
        public override IEnumerable<Note> ToNotes()
        {
            return Trainsets
                .Where(t => t.HasCoupleNote)
                .GroupBy(t => t.OperationDaysFlag)
                .Select(t => new Note
                {
                    DisplayOrder = 20000 + t.Key.DisplayOrder(),
                    Text = FormatText(t)
                });
        }
        private string FormatText(IGrouping<byte, Trainset> t) => TrainInfo != null && TrainInfo.OperationDays.Equals(t.Key.OperationDays())
                ? string.Format(CultureInfo.CurrentUICulture, Notes.ConnectTrainset, string.Join(",", t))
                : t.Key.OperationDays().ShortName + ": " + string.Format(CultureInfo.CurrentUICulture, Notes.ConnectTrainset, string.Join(",", t));
    }

    public class TrainsetsArrivalCallNote : TrainsetsCallNote
    {
        public TrainsetsArrivalCallNote(int callId) : base(callId)
        {
            IsForArrival = true;
        }

        protected override void UpdateVisibility()
        {
            base.UpdateVisibility();
            IsDriverNote = Trainsets.Any(t => t.HasUncoupleNote);
        }

        public override IEnumerable<Note> ToNotes()
        {
            return Trainsets
                .Where(t => t.HasUncoupleNote)
                .GroupBy(t => t.OperationDaysFlag)
                .Select(t => new Note
                {
                    DisplayOrder = 20000 + t.Key.DisplayOrder(),
                    Text = FormatText(t)
                });
        }

        private string FormatText(IGrouping<byte, Trainset> t) => TrainInfo != null && TrainInfo.OperationDays.Equals(t.Key.OperationDays())
                ? string.Format(CultureInfo.CurrentUICulture, Notes.DisconnectTrainset, string.Join(",", t))
                : t.Key.OperationDays().ShortName + ": " + string.Format(CultureInfo.CurrentUICulture, Notes.DisconnectTrainset, string.Join(",", t));
    }

    public class Trainset
    {
        public string Operator { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Class { get; set; } = string.Empty;
        public string? Note { get; set; }
        public bool IsCargo { get; set; }
        public int PositionInTrain { get; set; }
        public int MaxNumberOfWaggons { get; set; }
        public byte OperationDaysFlag { get; set; }
        public bool HasCoupleNote { get; set; }
        public bool HasUncoupleNote { get; set; }
        public override string ToString() => $"{Operator} {Number} {Note}".TrimEnd();
    }


}
