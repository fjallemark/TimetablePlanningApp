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
        public abstract IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays);

        public bool IsAnyDay(byte days, byte dutyDays = OperationDays.AllDays) =>
           dutyDays.IsOnDemand() || TrainInfo.IsOnDemand() || Days(days, dutyDays) > 0;

        public bool IsNoDay(byte days, byte dutyDays = OperationDays.AllDays) =>
            !dutyDays.IsOnDemand() && !TrainInfo.IsOnDemand() && Days(days, dutyDays) == 0;

        public bool IsAllDays(byte days, byte dutyDays = OperationDays.AllDays) =>
            dutyDays.IsOnDemand() || TrainInfo.IsOnDemand() || Days(days, dutyDays) == dutyDays;

        protected byte Days(byte days, byte dutyDays = OperationDays.AllDays) =>
            Days(days, TrainInfo!.OperationDaysFlags, dutyDays);

        private static byte Days(byte days, byte trainDays, byte dutyDays = OperationDays.AllDays) =>
            trainDays == OperationDays.OnDemand ? OperationDays.OnDemand :
            (byte)(days & trainDays & dutyDays);
    }

    public sealed class ManualTrainCallNote : TrainCallNote
    {
        // TODO: Implemnent support for multilanguage manual notes.
        public ManualTrainCallNote(int callId) : base(callId) { }
        public byte OperationDayFlag { get; set; } = OperationDays.AllDays;
        public string Text { get; set; } = string.Empty;
        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
            IsAnyDay(OperationDayFlag, onlyDays) ? Note.SingleNote(DisplayOrderWithDays(onlyDays), GetNoteText(onlyDays)) : Enumerable.Empty<Note>();

        private readonly List<LocalizedManualTrainCallNote> LocalizedNotes = new();
        public void AddLocalizedManualTrainCallNote(LocalizedManualTrainCallNote localizedNote) => LocalizedNotes.Add(localizedNote);
        string GetNoteText(byte onlyDays)
        {
            if (LocalizedNotes.Count == 0)
            {
                return FormattedNoteText(Text, onlyDays);
            }
            else
            {
                var localizedNote = LocalizedNotes.FirstOrDefault(x => x.LanguageCode == CultureInfo.CurrentCulture.TwoLetterISOLanguageName);
                if (localizedNote is null || string.IsNullOrWhiteSpace(localizedNote.Text)) return FormattedNoteText(Text, onlyDays); ;
                return FormattedNoteText(localizedNote.Text, onlyDays);
            }
        }

        string FormattedNoteText(string text, byte onlyDays) => $"{GetNoteDays(onlyDays)}{text}";

        string GetNoteDays(byte onlyDays)
        {
            if (IsAllDays(OperationDayFlag, onlyDays)) return string.Empty;
            if (IsAnyDay(OperationDayFlag, onlyDays)) return $"{Days(OperationDayFlag, onlyDays).OperationDays().ShortName}: ";
            return string.Empty;
        }

        int DisplayOrderWithDays(byte onlyDays) => DisplayOrder + Days(OperationDayFlag, onlyDays);
    }

    public sealed record LocalizedManualTrainCallNote(string LanguageCode, string? Text);

    public abstract class TrainsetsCallNote : TrainCallNote
    {
        protected TrainsetsCallNote(int callId) : base(callId)
        {
        }
        protected IList<Trainset> Trainsets { get; } = new List<Trainset>();
        public bool IsCargoOnly { init; get; }
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
        protected IEnumerable<Note> ToNotes(Func<Trainset, bool> criteria, byte dutyDays = OperationDays.AllDays)
        {
            return Merge(Trainsets.Where(t => criteria(t) && IsAnyDay(t.OperationDaysFlag, dutyDays)))
                .OrderBy(t => t.PositionInTrain)
                .GroupBy(t => Days(t.OperationDaysFlag, dutyDays))
                .Select(t => new Note
                {
                    DisplayOrder = 1000 + t.Key.DisplayOrder() + t.Key,
                    Text = Text(t.Key, dutyDays, t)
                });
        }
        protected static IEnumerable<Trainset> Merge(IEnumerable<Trainset> trainsets)
        {
            return trainsets
                .GroupBy(ts => ts.Number)
                .Select(ts => new Trainset
                {
                    Operator = ts.First().Operator,
                    Number = ts.First().Number,
                    WagonTypes = ts.First().WagonTypes,
                    OperationDaysFlag = (byte)ts.Sum(x => x.OperationDaysFlag),
                    HasCoupleNote = ts.First().HasCoupleNote,
                    HasUncoupleNote = ts.First().HasUncoupleNote
                });
        }
        protected string Text(byte days, byte dutyDays, IEnumerable<Trainset> trainsets) =>
             IsAllDays(days, dutyDays) ? Text(trainsets) : $"{days.OperationDays()}: {Text(trainsets)}";

        protected abstract string Text(IEnumerable<Trainset> t);

        protected static string TrainsetTexts(IEnumerable<Trainset> trainsets) =>
            string.Join(" ", trainsets.Select(ts => TrainsetFormat(ts)));

        // TODO: Add final destination and note about exchange trainset under way.
        private static string TrainsetFormat(Trainset ts) =>
            ts.Operator.HasValue() ? $"[{ts.Operator} {ts.WagonTypes.ToLowerInvariant()} {Notes.VehicleScheduleNumber.ToLowerInvariant()} {ts.Number}]" :
            $"[{ts.WagonTypes} {Notes.VehicleScheduleNumber.ToLowerInvariant()} {ts.Number}]";
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

        public override IEnumerable<Note> ToNotes(byte dutyDays = OperationDays.AllDays) =>
            ToNotes(t => t.HasCoupleNote, dutyDays);

        protected override string Text(IEnumerable<Trainset> trainsets) =>
            string.Format(CultureInfo.CurrentCulture, Action, TrainsetTexts(trainsets));

        private string Action => IsCargoOnly ? Notes.Load : Notes.ConnectTrainset;
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

        protected override string Text(IEnumerable<Trainset> trainsets) =>
            string.Format(CultureInfo.CurrentCulture, Action, TrainsetTexts(trainsets));
        private string Action => IsCargoOnly ? Notes.Unload : Notes.DisconnectTrainset;

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
            Note.SingleNote(320, FormatText(ContinuingTrain.OperationDayFlag, onlyDays));


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
                    DisplayOrder + Days(MeetingTrains.First().OperationDayFlag, MeetingTrains.Last().OperationDayFlag),
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
            DisplayOrder = -100;
        }

        public Loco ArrivingLoco { get; set; } = Loco.Empty;
        public Loco DepartingLoco { get; set; } = Loco.Empty;
        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
        {
            var days = (byte)(ArrivingLoco.OperationDaysFlags & DepartingLoco.OperationDaysFlags & onlyDays);
            return days > 0 || ArrivingLoco.OperationDaysFlags.IsOnDemand() || DepartingLoco.OperationDaysFlags.IsOnDemand() ?
                Note.SingleNote(DisplayOrder, string.Format(CultureInfo.CurrentCulture, Notes.EngineChange, ArrivingLoco.DisplayFormat(), DepartingLoco.DisplayFormat()), days) :
                Array.Empty<Note>();
        }
    }



    public abstract class LocoCallNote : TrainCallNote
    {
        protected LocoCallNote(int callId) : base(callId)
        {
            IsDriverNote = true;
            IsStationNote = true;
            DisplayOrder = -303;
        }

        protected abstract string ParkingText(byte days, byte onlydays);

        protected static string LocoText(string format, Loco loco) =>
           string.Format(CultureInfo.CurrentCulture, format, loco.DisplayFormat(), loco.OperationDays());
    }

    public class LocoDepartureCallNote : LocoCallNote
    {
        public LocoDepartureCallNote(int callId) : base(callId)
        {
            IsDriverNote = true;
            IsStationNote = true;
            IsForDeparture = true;
            DisplayOrder = -303;
        }

        public Loco DepartingLoco { get; set; } = Loco.Empty;
        public bool IsFromParking { get; set; }

        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays)
        {
            var days = (byte)(onlyDays & TrainInfo!.OperationDaysFlags);
            if (days == 0) days = OperationDays.OnDemand;
            return IsAnyDay(DepartingLoco.OperationDaysFlags, onlyDays) ?
                Note.SingleNote(DisplayOrder + Days(DepartingLoco.OperationDaysFlags, onlyDays), ParkingText(DepartingLoco.OperationDaysFlags, days)) :
                Array.Empty<Note>();
        }

        protected override string ParkingText(byte days, byte onlydays) =>
            IsAllDays(days, onlydays) ?
                IsFromParking ?
                    LocoText(Notes.GetLocoAtParking, DepartingLoco) :
                LocoText(Notes.UseLoco, DepartingLoco) :
           IsFromParking ?
                LocoText(Notes.GetLocoAtParkingOnDays, DepartingLoco) :
           LocoText(Notes.UseLocoAtDays, DepartingLoco);

    }

    public class LocoArrivalCallNote : LocoCallNote
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
            var days = Days(ArrivingLoco.OperationDaysFlags, onlyDays);
            var parkingNote = ParkingText(ArrivingLoco.OperationDaysFlags, onlyDays);

            if (parkingNote.HasValue())
            {
                result.Add(new Note { DisplayOrder = GetDisplayOrder(onlyDays, 3600), Text = parkingNote });
            }
            return result;
        }

        protected override string ParkingText(byte days, byte onlyDays) =>
            IsToParking ?
                IsAllDays(days, onlyDays) ?
                    LocoText(Notes.PutLocoAtParking, ArrivingLoco) :
                    LocoText(Notes.PutLocoAtParkingAtDays, ArrivingLoco) :
            string.Empty;

        protected int GetDisplayOrder(byte onlyDays, int sortOrder) => sortOrder + Days(ArrivingLoco.OperationDaysFlags, onlyDays);
    }

    public class LocoCirculationNote : LocoArrivalCallNote
    {
        public LocoCirculationNote(int callId) : base(callId) { }

        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
            IsNoDay(ArrivingLoco.OperationDaysFlags, onlyDays) ? Enumerable.Empty<Note>() :
            Note.SingleNote(GetDisplayOrder(onlyDays, 3300), CirculateText(ArrivingLoco.OperationDaysFlags, onlyDays));

        private string CirculateText(byte days, byte onlydays) =>
            CirculateLoco ?
                IsAllDays(days, onlydays) ? Notes.CirculateLoco :
                string.Format(Notes.CirculateLocoDays, Days(ArrivingLoco.OperationDaysFlags, onlydays).OperationDays().ShortName) :
                string.Empty;
    }

    public class LocoTurnNote : LocoArrivalCallNote
    {
        public LocoTurnNote(int callId) : base(callId) { }
        public override IEnumerable<Note> ToNotes(byte onlyDays = OperationDays.AllDays) =>
            IsNoDay(ArrivingLoco.OperationDaysFlags, onlyDays) ? Enumerable.Empty<Note>() :
            Note.SingleNote(GetDisplayOrder(onlyDays, 3000), Notes.TurnLoco);

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
        public int StationId { get; set; }
        public string StationName { get; set; } = string.Empty;
        public string? TransferDestinationName { get; set; }
        public bool ToAllDestinations { get; set; }
        public bool AndBeyond { get; set; }
        public int OrderInTrain { get; set; }
        public int MaxNumberOfWagons { get; set; }
        public bool TransferAndBeyond { get; set; }
        public string DestinationCountryName { get; set; } = string.Empty;
        public bool IsInternational { get; set; }
        public bool IsRegion { get; set; }
        public bool HasCouplingNote { get; set; }
        public bool HasUncouplingNote { get; set; }
        public string ForeColor { get; set; } = "#000000";
        public string BackColor { get; set; } = "#FFFFFF";
        public override string ToString() =>
            IsRegion ? DestinationText :
            ToAllDestinations ? AllDestinations :
            AndBeyond || TransferAndBeyond ? string.Format(CultureInfo.CurrentCulture, Notes.AndBeyond, DestinationText) :
            DestinationText;

        internal string AllDestinations => UseDestinationCountry ? string.Format(Notes.DestinationInCountry, Notes.AllDestinations, DestinationCountryName) : Notes.AllDestinations;
        internal bool UseDestinationCountry => HasDestinationCountry && IsInternational;
        internal bool HasDestinationCountry => !string.IsNullOrWhiteSpace(DestinationCountryName);
        internal string FinalDestinationStationName => IsRegion ? StationName : string.IsNullOrWhiteSpace(TransferDestinationName) ? StationName : TransferDestinationName;
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
            var destinationGroups = me.OrderBy(dg => dg.OrderInTrain).GroupBy(bd => bd.StationId * 1000 + bd.OrderInTrain);
            foreach (var destinationGroup in destinationGroups)
            {
                var text = new StringBuilder(200);
                var destinations = destinationGroup.OrderBy(dg => dg.MaxNumberOfWagons).ToArray();
                var destinationTextsInGroup = destinations.Select(d => d.ToString()).Distinct();
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
        public string StationName => StationNames.Count > 0 ? StationNames[0] : string.Empty;
        public IList<string> StationNames { get; } = new List<string>();
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
            string.Format(CultureInfo.CurrentCulture, Notes.DisconnectWagonsToHere, string.Join(", ", StationNames));
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

    public class Trainset
    {
        public string Operator { get; set; } = string.Empty;
        public int Number { get; set; }
        public string Class { get; set; } = string.Empty;
        public string WagonTypes { get; set; } = string.Empty;
        public bool IsCargo { get; set; }
        public int PositionInTrain { get; set; }
        public int MaxNumberOfWaggons { get; set; }
        public byte OperationDaysFlag { get; set; }
        public bool HasCoupleNote { get; set; }
        public bool HasUncoupleNote { get; set; }
        public string Destination { get; set; } = string.Empty;
        public string FinalDestination { get; set; } = string.Empty;
        public bool HasFinalDestination => FinalDestination.HasValue() && FinalDestination != Destination;
        public override string ToString() => $"{Operator} {Number} {WagonTypes}".TrimEnd();
    }
}
