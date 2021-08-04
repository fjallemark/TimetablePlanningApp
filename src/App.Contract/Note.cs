using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Tellurian.Trains.Planning.App.Contracts.Resources;

namespace Tellurian.Trains.Planning.App.Contracts
{
    public class Note
    {
        public string Text { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public byte OperatingDaysFlag { get; set; } = 0x7F;
        public override string ToString() => Text ?? "";
        public static Note[] SingleNote(int displayOrder, string text, byte operationDaysFlag = 0x7F) => new[] { new Note { DisplayOrder = displayOrder, Text = text, OperatingDaysFlag = operationDaysFlag } };
        public override bool Equals(object? obj) => obj is Note other && other.Text.Equals(Text, StringComparison.OrdinalIgnoreCase);
        public override int GetHashCode() => Text.GetHashCode(StringComparison.OrdinalIgnoreCase);
        public bool IsValidDays(byte operatingDaysFlag) => OperatingDaysFlag.And(operatingDaysFlag) > 0;
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
        public abstract IEnumerable<Note> ToNotes(byte onlyDays);

        public bool IsAnyDay(byte days, byte dutyDays = OperationDays.AllDays) => Days(days, dutyDays) > 0;
        public bool IsNoDay(byte days, byte dutyDays = OperationDays.AllDays) => !IsAnyDay(days, dutyDays);
        public bool IsAllDays(byte days, byte dutyDays = OperationDays.AllDays) => Days(days, dutyDays) == dutyDays;
        public byte Days(byte days, byte dutyDays = OperationDays.AllDays) => Days(days, TrainInfo!.OperationDaysFlags, dutyDays);
        public static byte Days(byte days, byte trainDays, byte dutyDays = OperationDays.AllDays) => (byte)(days & trainDays & dutyDays);
    }

    public sealed class ManualTrainCallNote : TrainCallNote
    {
        // TODO: Implemnent support for multilanguage manual notes.
        public ManualTrainCallNote(int callId) : base(callId) { }
        public string Text { get; set; } = string.Empty;
        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) => Note.SingleNote(DisplayOrder, Text);
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
        protected IEnumerable<Note> ToNotes(Func<NoteTrainset, bool> criteria, byte dutyDays = OperationDays.AllDays)
        {
            return Merge(Trainsets.Where(t => criteria(t) && IsAnyDay(t.OperationDaysFlag, dutyDays)))
                .OrderBy(t => t.PositionInTrain)
                .GroupBy(t => Days(t.OperationDaysFlag, dutyDays))
                .Select(t => new Note
                {
                    DisplayOrder = 10000 + t.Key.DisplayOrder(),
                    Text = Text(t.Key, dutyDays, t)
                });
        }
        protected static IEnumerable<NoteTrainset> Merge(IEnumerable<NoteTrainset> trainsets)
        {
            return trainsets
                .GroupBy(ts => ts.Number)
                .Select(ts => new NoteTrainset
                {
                    Operator = ts.First().Operator,
                    Number = ts.First().Number,
                    Note = ts.First().Note,
                    OperationDaysFlag = (byte)ts.Sum(x => x.OperationDaysFlag),
                    HasCoupleNote = ts.First().HasCoupleNote,
                    HasUncoupleNote = ts.First().HasUncoupleNote
                });
        }
        protected string Text(byte days, byte dutyDays, IEnumerable<NoteTrainset> t) =>
             IsAllDays(days, dutyDays) ? Text(t) : $"{days.OperationDays()}: {Text(t)}";

        protected abstract string Text(IEnumerable<NoteTrainset> t);

        protected static string TrainsetTexts(IEnumerable<NoteTrainset> trainsets) => string.Join(" ", trainsets.Select(ts => TrainsetFormat(ts)));
        private static string TrainsetFormat(NoteTrainset ts) => $"[{ts.Operator}{ts.Number}]";
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

        public override IEnumerable<Note> ToNotes(byte dutyDays = OperationDays.AllDays) => ToNotes(t => t.HasCoupleNote, dutyDays);

        protected override string Text(IEnumerable<NoteTrainset> trainsets) => string.Format(CultureInfo.CurrentCulture, Notes.ConnectTrainset, TrainsetTexts(trainsets));
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


        public override IEnumerable<Note> ToNotes(byte dutyDays = OperationDays.AllDays) => ToNotes(t => t.HasUncoupleNote, dutyDays);

        protected override string Text(IEnumerable<NoteTrainset> trainsets) => string.Format(CultureInfo.CurrentCulture, Notes.DisconnectTrainset, TrainsetTexts(trainsets));
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

        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
            ContinuingTrain is null || IsNoDay(ContinuingTrain.OperationDayFlag, onlyDays) ?
            Array.Empty<Note>() :
            Note.SingleNote(32000, FormatText(ContinuingTrain.OperationDayFlag, onlyDays));


        private string FormatText(byte days, byte onlyDays) =>
            IsAllDays(days, onlyDays) ?
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
        public IList<OtherTrainCall> MeetingTrains { get; } = new List<OtherTrainCall>();
        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
        {
            var meetingTrains = ActualMeetingTrains(onlyDays);
            if (meetingTrains.Any())
            {
                return Note.SingleNote(
                    DisplayOrder,
                    string.Format(CultureInfo.CurrentCulture, Notes.MeetsOtherTrains,
                    string.Join(", ", meetingTrains.Distinct()
                    .OrderBy(mt => mt.MeetArrivalTime.OffsetMinutes()).Select(mt => mt.MeetingTrainInfo((byte)(OperationDayFlag & onlyDays))))));
            }
            else
            {
                return Array.Empty<Note>();
            }
        }

        private IEnumerable<OtherTrainCall> ActualMeetingTrains(byte onlyDays) => MeetingTrains.Where(mt => (mt.OperationDayFlag & onlyDays) > 0);
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
        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
        {
            var days = (byte)(ArrivingLoco.OperationDaysFlags & DepartingLoco.OperationDaysFlags & onlyDays);
            return days == 0 ? Array.Empty<Note>() : 
                Note.SingleNote(DisplayOrder, string.Format(CultureInfo.CurrentCulture, Notes.EngineChange, ArrivingLoco, DepartingLoco), days);
        }
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

        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
        {
            var days = (byte)(onlyDays & TrainInfo!.OperationDaysFlags);
            return IsNoDay(DepartingLoco.OperationDaysFlags, days) ? Array.Empty<Note>() :
                Note.SingleNote(DisplayOrder, FormatText(DepartingLoco.OperationDaysFlags, days));
        }

        private string FormatText(byte days, byte onlydays) => IsAllDays(days, onlydays) ?
            IsFromParking ?
            string.Format(CultureInfo.CurrentCulture, Notes.GetLocoAtParking, LocoDescription, DepartingLoco.OperatorName, DepartingLoco.Number) :
            string.Format(CultureInfo.CurrentCulture, Notes.UseLoco, LocoDescription, DepartingLoco.OperatorName, DepartingLoco.Number) :
            IsFromParking ?
            string.Format(CultureInfo.CurrentCulture, Notes.GetLocoAtParkingOnDays, LocoDescription, DepartingLoco.OperatorName, DepartingLoco.Number, DepartingLoco.OperationDaysFlags.OperationDays()) :
            string.Format(CultureInfo.CurrentCulture, Notes.UseLocoAtDays, LocoDescription, DepartingLoco.OperatorName, DepartingLoco.Number, DepartingLoco.OperationDaysFlags.OperationDays());

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

        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
        {
            var result = new List<Note>();
            if (IsNoDay(ArrivingLoco.OperationDaysFlags, onlyDays)) return result;
            var parkingNote = ToParkingNote(ArrivingLoco.OperationDaysFlags, onlyDays);
            if (parkingNote != null)
            {
                result.Add(new Note { DisplayOrder = 3000, Text = TurnLoco ? $"{parkingNote} {Notes.TurnLoco}" : parkingNote });
            }
            else
            {
                if (TurnLoco) result.Add(new Note { DisplayOrder = 3001, Text = Notes.TurnLoco });
            }
            if (CirculateLoco) result.Add(new Note { DisplayOrder = 3002, Text = Notes.CirculateLoco });
            return result;
        }

        private string? ToParkingNote(byte days, byte onlyDays) =>
            IsToParking ?
            IsAllDays(days, onlyDays) ?
            string.Format(CultureInfo.CurrentCulture, Notes.PutLocoAtParking, ArrivingLoco.OperatorName, ArrivingLoco.Number) :
            string.Format(CultureInfo.CurrentCulture, Notes.PutLocoAtParkingAtDays, ArrivingLoco.OperatorName, ArrivingLoco.Number, ArrivingLoco.OperationDaysFlags.OperationDays()) :
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

        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
            Note.SingleNote(100, string.Format(CultureInfo.CurrentCulture, Notes.BringsWaggonsToDestinations, BlockDestinations.DestinationText(true)));
    }

    public class BlockDestination
    {
        public string StationName { get; set; } = string.Empty;
        public string? TransferDestinationName { get; set; }
        public bool ToAllDestinations { get; set; }
        public bool AndBeyond { get; set; }
        public int OrderInTrain { get; set; }
        public int MaxNumberOfWagons { get; set; }
        public bool TransferAndBeyond { get; set; }
        public string DestinationCountryName { get; set; } = string.Empty;
        public bool IsInternational { get; set; }
        public bool HasCouplingNote { get; set; }
        public bool HasUncouplingNote { get; set; }
        public string ForeColor { get; set; } = "#000000";
        public string BackColor { get; set; } = "#FFFFFF";
        public override string ToString() =>
            ToAllDestinations ? AllDestinations :
            AndBeyond || TransferAndBeyond ? string.Format(CultureInfo.CurrentCulture, Notes.AndBeyond, DestinationText) :
            DestinationText;

        internal string AllDestinations => UseDestinationCountry ? string.Format(Notes.DestinationInCountry, Notes.AllDestinations, DestinationCountryName) : Notes.AllDestinations;
        internal bool UseDestinationCountry => HasDestinationCountry && IsInternational;
        internal bool HasDestinationCountry => !string.IsNullOrWhiteSpace(DestinationCountryName);
        internal string FinalDestinationStationName => string.IsNullOrWhiteSpace(TransferDestinationName) ? StationName : TransferDestinationName;
        internal string DestinationText => UseDestinationCountry ? string.Format(Notes.DestinationInCountry, FinalDestinationStationName, DestinationCountryName) : FinalDestinationStationName;

        public override bool Equals(object? obj) => obj is BlockDestination other && other.ToString().Equals(ToString(), StringComparison.OrdinalIgnoreCase);
        public override int GetHashCode() => ToString().GetHashCode(StringComparison.OrdinalIgnoreCase);
    }

    public static class BlockDestinationsExtensions
    {
        public static string DestinationText(this IEnumerable<BlockDestination> me, bool useBrackets = false) =>
            string.Join(", ", me.GroupText(useBrackets));
        public static IEnumerable<string> GroupText(this IEnumerable<BlockDestination> me, bool useBrackets = false)
        {
            var result = new List<string>();
            var destinationGroups = me.OrderBy(dg => dg.OrderInTrain).GroupBy(bd => bd.OrderInTrain);
            foreach (var destinationGroup in destinationGroups)
            {
                var text = new StringBuilder(200);
                var destinations = destinationGroup.OrderBy(dg => dg.MaxNumberOfWagons).ToArray();
                var destinationTextsInGroup = destinations.Select(d => d.ToString());
                if (useBrackets) text.Append('[');
                text.Append(string.Join('|', destinationTextsInGroup));
                if (useBrackets) text.Append(']');
                var maxNumberOfWagons = destinations.Sum(d => d.MaxNumberOfWagons);
                if (maxNumberOfWagons > 0)
                {
                    text.Append('×');
                    text.Append(maxNumberOfWagons);
                }
                result.Add(text.ToString());
            }
            return result;
        }
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

        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
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
            string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHere, Notes.AllDestinations) :
            AndBeyond ? string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHereAndFurther, StationName) :
            string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHere, StationName);
    }



    public class OtherTrain
    {
        public string? CategoryPrefix { get; set; }
        public int TrainNumber { get; set; }
        public byte OperationDayFlag { get; set; }
        public string DestinationName { get; set; } = string.Empty;
        public string ContinuesAsTrainToDestination => string.Format(CultureInfo.CurrentCulture, Notes.ContinuesAsTrainToDestination, $"{CategoryPrefix} {TrainNumber}", DestinationName);
        public string ContinuesDaysAsTrainToDestination => string.Format(CultureInfo.CurrentCulture, Notes.ContinuesDaysAsTrainToDestination, $"{CategoryPrefix} {TrainNumber}", DestinationName, OperationDayFlag.OperationDays().ShortName);
        public virtual string MeetingTrainInfo(byte otherTrainDaysFlags)
        {
            byte commonDaysFlag = (byte)(OperationDayFlag & otherTrainDaysFlags);
            return commonDaysFlag == otherTrainDaysFlags ?
                $"{CategoryPrefix} {TrainNumber}" :
                $"{commonDaysFlag.OperationDays().ShortName} {CategoryPrefix} {TrainNumber}";
        }

        public override string ToString() => $"{OperationDayFlag.OperationDays().ShortName} {CategoryPrefix} {TrainNumber}";
    }

    public class OtherTrainCall : OtherTrain
    {
        public CallTime? ArrivalTime { get; set; }
        public CallTime? DepartureTime { get; set; }
        public CallTime? MeetArrivalTime { get; set; }
        public CallTime? MeetDepartureTime { get; set; }
        public override string MeetingTrainInfo(byte otherTrainDaysFlags)
        {
            byte commonDaysFlag = (byte)(OperationDayFlag & otherTrainDaysFlags);
            var arrivalTime = ArrivalTime.OffsetMinutes() > MeetArrivalTime.OffsetMinutes() ? ArrivalTime : MeetArrivalTime;
            var departureTime = DepartureTime.OffsetMinutes() < MeetDepartureTime.OffsetMinutes() ? DepartureTime : MeetDepartureTime;
            return commonDaysFlag == 0 ? string.Empty :
                commonDaysFlag == otherTrainDaysFlags ?
                $"{CategoryPrefix} {TrainNumber} {arrivalTime}-{departureTime}" :
                $"{commonDaysFlag.OperationDays().ShortName} {CategoryPrefix} {TrainNumber} {arrivalTime}-{departureTime}";
        }
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
