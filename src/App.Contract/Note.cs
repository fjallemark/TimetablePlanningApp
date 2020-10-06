using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;
using Tellurian.Trains.Planning.App.Contract.Resources;

#pragma warning disable CA1308 // Normalize strings to uppercase

namespace Tellurian.Trains.Planning.App.Contract
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
        protected IList<NoteTrainset> Trainsets { get; } = new List<NoteTrainset>();
        public void AddTrainset(NoteTrainset trainset)
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
        private string FormatText(IGrouping<byte, NoteTrainset> t) => TrainInfo?.OperationDays.Equals(t.Key.OperationDays()) == true
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

        private string FormatText(IGrouping<byte, NoteTrainset> t) => TrainInfo?.OperationDays.Equals(t.Key.OperationDays()) == true
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
        public OtherTrain ContinuingTrain { get; set; } = new OtherTrain();

        public byte LocoOperationDaysFlag { get; set; }
        public override IEnumerable<Note> ToNotes() =>
            ContinuingTrain is null ?
            Array.Empty<Note>() :
            Note.SingleNote(32000, FormatText);

        private string FormatText =>
            ContinuingTrain.OperationDayFlag == LocoOperationDaysFlag ?
            ContinuingTrain.ContinuesAsTrainToDestination :
            ContinuingTrain.ContinuesDaysAsTrainToDestination;
    }

    public class TrainMeetCallNote : TrainCallNote
    {
        public TrainMeetCallNote(int callId) : base(callId)
        {
            IsStationNote = true;
            IsDriverNote = true;
            IsForArrival = true;
            DisplayOrder = -500;
        }
        public byte OperationDayFlag { get; set; }
        public IList<OtherTrain> MeetingTrains { get; } = new List<OtherTrain>();
        public override IEnumerable<Note> ToNotes() =>
            Note.SingleNote(DisplayOrder, string.Format(CultureInfo.CurrentCulture, Notes.MeetsOtherTrains, string.Join(", ", MeetingTrains.Distinct().Select(mt => mt.MeetingTrainInfo(OperationDayFlag)))));
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
            string.Format(CultureInfo.CurrentCulture, Notes.GetLocoAtParking, LocoDescription, DepartingLoco.OperatorName, DepartingLoco.Number) :
            string.Format(CultureInfo.CurrentCulture, Notes.UseLoco, LocoDescription, DepartingLoco.OperatorName, DepartingLoco.Number) :
            IsFromParking ?
            string.Format(CultureInfo.CurrentCulture, Notes.GetLocoAtParkingOnDays, LocoDescription, DepartingLoco.OperatorName, DepartingLoco.Number, DepartingLoco.OperationDays) :
            string.Format(CultureInfo.CurrentCulture, Notes.UseLocoAtDays, LocoDescription, DepartingLoco.OperatorName, DepartingLoco.Number, DepartingLoco.OperationDays));

        private string LocoDescription =>
            DepartingLoco.IsRailcar ? Notes.Railcar.ToLowerInvariant() : Notes.Loco.ToLowerInvariant();
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
            string.Format(CultureInfo.CurrentCulture, Notes.BringsWaggonsToDestinations, string.Join(", ", BlockDestinations.Distinct().Select(d => d.ToString()))));
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
        public bool ToAllDestinations { get; set; }
        public bool AndBeyond { get; set; }
        public bool AlsoSwitch { get; set; }
        public int OrderInTrain { get; set; }

        public override IEnumerable<Note> ToNotes()
        {
            var result = new List<Note>();
            if (AlsoSwitch)
            {
                if (AndBeyond) result.Add(new Note { DisplayOrder = -801, Text = Notes.DisconnectContinuingWagons });
                result.Add(new Note { DisplayOrder = -800, Text = Notes.SwitchWagonsToCustomers });
            }
            else
            {
                result.Add(new Note { DisplayOrder = -700, Text = DisconnectNote });
            }
            return result;
        }

        private string DisconnectNote =>
            ToAllDestinations ?
            string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHere, Notes.AllDestinations.ToLowerInvariant()) :
            AndBeyond ? string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHereAndFurther, StationName) :
            string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHere, StationName);
    }

    public class BlockDestination
    {
        public string StationName { get; set; } = string.Empty;
        public bool ToAllDestinations { get; set; }
        public bool AndBeyond { get; set; }
        public int OrderInTrain { get; set; }
        public int MaxNumberOfWagons { get; set; }
        public string? TransferDestination { get; set; }
        public string ForeColor { get; set; } = "#000000";
        public string BackColor { get; set; } = "#FFFFFF";
        public override string ToString() =>
            ToAllDestinations ? Notes.AllDestinations.ToLowerInvariant() :
            AndBeyond ?
            string.Format(CultureInfo.CurrentCulture, Notes.AndBeyond, FinalDestinationNameWithMaxWagons) :
            FinalDestinationNameWithMaxWagons;

        public string FinalDestinationNameWithTransfer =>
            ToAllDestinations ? AllDestinationsWithMaxWagons :
            string.IsNullOrWhiteSpace(TransferDestination) ? StationName :
            string.Format(CultureInfo.CurrentCulture, Notes.TransferVia, TransferDestinationNameWithMaxWagons, StationName);


        public string FinalDestinationName =>
            ToAllDestinations ? AllDestinationsWithMaxWagons :
            string.IsNullOrWhiteSpace(TransferDestination) ? StationName :
            TransferDestination;

        public string FinalDestinationNameWithMaxWagons =>
            MaxNumberOfWagons == 0 ? FinalDestinationName :
            $"{FinalDestinationName}×{MaxNumberOfWagons}";

         private string AllDestinationsWithMaxWagons =>
            MaxNumberOfWagons == 0 ?
            Notes.AllDestinations.ToLowerInvariant() :
            $"{Notes.AllDestinations.ToLowerInvariant()}×{MaxNumberOfWagons}";

        private string TransferDestinationNameWithMaxWagons =>
            string.IsNullOrWhiteSpace(TransferDestination) ? string.Empty :
            MaxNumberOfWagons == 0 ? TransferDestination  :
            $"{TransferDestination}×{MaxNumberOfWagons}";

        public override bool Equals(object obj) => obj is BlockDestination other && other.ToString().Equals(ToString(), StringComparison.OrdinalIgnoreCase);
        public override int GetHashCode() => ToString().GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public class OtherTrain
    {
        public string? CategoryPrefix { get; set; }
        public int TrainNumber { get; set; }
        public byte OperationDayFlag { get; set; }
        public string DestinationName { get; set; } = string.Empty;
        public string ContinuesAsTrainToDestination => string.Format(CultureInfo.CurrentCulture, Notes.ContinuesAsTrainToDestination, $"{CategoryPrefix} {TrainNumber}", DestinationName);
        public string ContinuesDaysAsTrainToDestination => string.Format(CultureInfo.CurrentCulture, Notes.ContinuesDaysAsTrainToDestination, $"{CategoryPrefix} {TrainNumber}", DestinationName, OperationDayFlag.OperationDays().ShortName);
        public string MeetingTrainInfo(byte otherTrainDaysFlags)
        {
            byte commonDaysFlag = (byte)(OperationDayFlag & otherTrainDaysFlags);
            return commonDaysFlag == otherTrainDaysFlags ?
                $"{CategoryPrefix} {TrainNumber}" :
                $"{commonDaysFlag.OperationDays().ShortName} {CategoryPrefix} {TrainNumber}";
        }

        public override string ToString() => $"{OperationDayFlag.OperationDays().ShortName} {CategoryPrefix} {TrainNumber}";
    }

    public class NoteTrainset
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
