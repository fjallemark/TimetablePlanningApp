using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Tellurian.Trains.Planning.App.Shared.Resources;

namespace Tellurian.Trains.Planning.App.Shared
{
    public class Note
    {
        public string Text { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public override string ToString() => Text ?? "";
        public static Note[] SingleNote(int displayOrder, string text) => new[] { new Note { DisplayOrder = displayOrder, Text = text } };
        public override bool Equals(object obj) => obj is Note other && other.Text.Equals(Text, StringComparison.OrdinalIgnoreCase);
        public override int GetHashCode() => Text.GetHashCode(StringComparison.OrdinalIgnoreCase);
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
        public override IEnumerable<Note> ToNotes() => Note.SingleNote(DisplayOrder, Text);
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
                    DisplayOrder = 10000 + t.Key.DisplayOrder(),
                    Text = FormatText(t)
                });
        }
        private string FormatText(IGrouping<byte, Trainset> t) => TrainInfo?.OperationDays.Equals(t.Key.OperationDays()) == true
                ? string.Format(CultureInfo.CurrentCulture, Notes.ConnectTrainset, string.Join(",", t))
                : t.Key.OperationDays().ShortName + ": " + string.Format(CultureInfo.CurrentCulture, Notes.ConnectTrainset, string.Join(",", t));
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
                    DisplayOrder = -1000 + t.Key.DisplayOrder(),
                    Text = FormatText(t)
                });
        }

        private string FormatText(IGrouping<byte, Trainset> t) => TrainInfo?.OperationDays.Equals(t.Key.OperationDays()) == true
                ? string.Format(CultureInfo.CurrentCulture, Notes.DisconnectTrainset, string.Join(",", t))
                : t.Key.OperationDays().ShortName + ": " + string.Format(CultureInfo.CurrentCulture, Notes.DisconnectTrainset, string.Join(",", t));
    }

    public class TrainContinuationNumberCallNote : TrainCallNote
    {
        public TrainContinuationNumberCallNote(int callId) : base(callId)
        {
            IsStationNote = true;
            IsDriverNote = true;
            IsForArrival = true;
        }
        public ContinuingTrain ContinuingTrain { get; set; } = new ContinuingTrain();

        public byte LocoOperationDaysFlag { get; set; }
        public override IEnumerable<Note> ToNotes() =>
            ContinuingTrain is null ?
            Array.Empty<Note>() :
            Note.SingleNote(32000, FormatText);

        private string FormatText =>
            ContinuingTrain.OperationDayFlag == LocoOperationDaysFlag ?
            ContinuingTrain.TrainAndDestination :
            ContinuingTrain.DayTrainAndDestination;
    }

    public class LocoExchangeCallNote : TrainCallNote
    {
        public LocoExchangeCallNote(int callId) : base(callId)
        {
            IsStationNote = true;
            IsDriverNote = true;
            IsForArrival = true;
            DisplayOrder = -1000;
        }

        public Loco ArrivingLoco { get; set; } = Loco.Empty;
        public Loco DepartingLoco { get; set; } = Loco.Empty;
        public override IEnumerable<Note> ToNotes() => Note.SingleNote(DisplayOrder, string.Format(CultureInfo.CurrentCulture, Notes.EngineChange, ArrivingLoco, DepartingLoco));
    }

    public class LocoDepartureCallNote : TrainCallNote
    {
        public LocoDepartureCallNote(int callId) : base(callId)
        {
            IsDriverNote = true;
            IsStationNote = true;
            IsForDeparture = true;
            DisplayOrder = -30003;
        }

        public Loco DepartingLoco { get; set; } = Loco.Empty;
        public bool IsFromParking { get; set; }
        public OperationDays TrainOperationDays { get; set; } = new OperationDays();

        public override IEnumerable<Note> ToNotes() => Note.SingleNote(DisplayOrder,
            TrainOperationDays.Equals(DepartingLoco.OperationDays) ?
            IsFromParking ?
            string.Format(CultureInfo.CurrentCulture, Notes.GetLocoAtParking, DepartingLoco.OperatorName, DepartingLoco.Number) :
            string.Format(CultureInfo.CurrentCulture, Notes.UseLoco, DepartingLoco.OperatorName, DepartingLoco.Number) :
            IsFromParking ?
            string.Format(CultureInfo.CurrentCulture, Notes.GetLocoAtParkingOnDays, DepartingLoco.OperatorName, DepartingLoco.Number, DepartingLoco.OperationDays) :
            string.Format(CultureInfo.CurrentCulture, Notes.UseLocoAtDays, DepartingLoco.OperatorName, DepartingLoco.Number, DepartingLoco.OperationDays));
    }

    public class LocoArrivalCallNote : TrainCallNote
    {
        public LocoArrivalCallNote(int callId) : base(callId)
        {
            IsDriverNote = true;
            IsStationNote = true;
            IsForArrival = true;
        }
        public Loco ArrivingLoco { get; set; } = Loco.Empty;
        public bool IsToParking { get; set; }
        public bool TurnLoco { get; set; }
        public bool CirculateLoco { get; set; }
        public OperationDays TrainOperationDays { get; set; } = new OperationDays();

        public override IEnumerable<Note> ToNotes()
        {
            var result = new List<Note>();
            if (TurnLoco) result.Add(new Note { DisplayOrder = 3001, Text = Notes.TurnLoco });
            if (CirculateLoco) result.Add(new Note { DisplayOrder = 3002, Text = Notes.CirculateLoco });
            var parkingNote = FromParkingNote;
            if (parkingNote != null) result.Add(new Note { DisplayOrder = 3003, Text = parkingNote });
            return result;
        }

        private string? FromParkingNote =>
            IsToParking ?
            TrainOperationDays.Equals(ArrivingLoco.OperationDays) ?
            string.Format(CultureInfo.CurrentCulture, Notes.PutLocoAtParking, ArrivingLoco.OperatorName, ArrivingLoco.Number) :
            string.Format(CultureInfo.CurrentCulture, Notes.PutLocoAtParkingAtDays, ArrivingLoco.OperatorName, ArrivingLoco.Number, ArrivingLoco.OperationDays) :
            null;
    }

    public class BlockDestinationsCallNote : TrainCallNote
    {
        public BlockDestinationsCallNote(int callId) : base(callId)
        {
            IsShuntingNote = true;
            IsDriverNote = true;
            IsForDeparture = true;
        }

        public IList<BlockDestination> BlockDestinations { get; } = new List<BlockDestination>();

        public override IEnumerable<Note> ToNotes() => Note.SingleNote(100,
            string.Format(CultureInfo.CurrentCulture, Notes.BringsWaggonsToDestinations, string.Join(", ", BlockDestinations.Select(d => d.ToString()))));
    }

    public class BlockArrivalCallNote : TrainCallNote
    {
        public BlockArrivalCallNote(int callId) : base(callId)
        {
            IsShuntingNote = true;
            IsDriverNote = true;
            IsForArrival = true;

        }
        public string StationName { get; set; } = string.Empty;
        public bool AndBeyond { get; set; }
        public bool AlsoSwitch { get; set; }
        public int OrderInTrain { get; set; }

        public override IEnumerable<Note> ToNotes()
        {
            var result = new List<Note>();
            if (AlsoSwitch)
            {
                if (AndBeyond) result.Add(new Note { DisplayOrder = -801, Text = Notes.DisconnectContinuingWagons });
                result.Add(new Note { DisplayOrder = -800, Text = Notes.SwitchWagonsWithMainLoco });
            }
            else
            {
                if (AndBeyond) result.Add(new Note { DisplayOrder = -700, Text = string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHereAndFurther, StationName) });
                else result.Add(new Note { DisplayOrder = -700, Text = string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHere, StationName)});
            }
            return result;
        }
    }

    public class BlockDestination
    {
        public string Name { get; set; } = string.Empty;
        public bool AndBeyond { get; set; }
        public int OrderInTrain { get; set; }
        public override string ToString() =>
            AndBeyond ?
            string.Format(CultureInfo.CurrentCulture, Notes.AndBeyond, Name) :
            Name;
    }

    public class ContinuingTrain
    {
        public string? CategoryPrefix { get; set; }
        public int TrainNumber { get; set; }
        public byte OperationDayFlag { get; set; }
        public string DestinationName { get; set; } = string.Empty;
        public string TrainAndDestination => string.Format(CultureInfo.CurrentCulture, Notes.ContinuesAsTrainToDestination, $"{CategoryPrefix} {TrainNumber}", DestinationName);
        public string DayTrainAndDestination => string.Format(CultureInfo.CurrentCulture, Notes.ContinuesDaysAsTrainToDestination, $"{CategoryPrefix} {TrainNumber}", DestinationName, OperationDayFlag.OperationDays().ShortName);
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
